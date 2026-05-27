using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.URPRenderFeatures.SpriteRendererSilhouetteRenderFeature.Scripts
{
    public class SpriteRendererOutlinePass : ScriptableRenderPass
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexUVs = Shader.PropertyToID("_MainTex_UVs");
        private const int MAX_INSTANC_PER_BLOCK = 1023;
        
        private static readonly List<SpriteRenderer> _cachedRenderers = new List<SpriteRenderer>();
        private readonly Material _outlineMaterial;
        private readonly Mesh _quadMesh;
    
        // Kept as a persistent field to avoid garbage collection allocations every frame
        private readonly MaterialPropertyBlock _propertyBlock;
    
        // Use dictionaries to group by texture
        private Dictionary<Texture2D, List<Matrix4x4>> batchedMatrices = new Dictionary<Texture2D, List<Matrix4x4>>();
        private Dictionary<Texture2D, List<Vector4>> batchedUVs = new Dictionary<Texture2D, List<Vector4>>();
   
        // Pre-calculated offset directions to avoid runtime math
        private readonly Vector3[] _outlineOffsets = new Vector3[8];
    
        public static void RegisterRenderer(SpriteRenderer renderer)
        {
            _cachedRenderers.Add(renderer);
            _cachedRenderers.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
        }
        
        public static void UnregisterRenderer(SpriteRenderer renderer)
        {
            _cachedRenderers.Remove(renderer);
        }
    
        public SpriteRendererOutlinePass(Material material, float outlineThickness, RenderPassEvent renderPassEvent)
        {
            this._outlineMaterial = material;
            this.renderPassEvent = renderPassEvent;
            this._quadMesh = CreateQuadMesh();
            this._propertyBlock = new MaterialPropertyBlock(); 

            // Pre-calculate the 8 directional offsets (normalized so diagonal outlines aren't thicker)
            Vector2[] dirs = {
                new Vector2(0, 1), new Vector2(1, 1).normalized, new Vector2(1, 0), new Vector2(1, -1).normalized,
                new Vector2(0, -1), new Vector2(-1, -1).normalized, new Vector2(-1, 0), new Vector2(-1, 1).normalized
            };
        
            for (int i = 0; i < 8; i++)
            {
                _outlineOffsets[i] = new Vector3(dirs[i].x * outlineThickness, dirs[i].y * outlineThickness, 0f);
            }
        }

        // --- UNITY 6 RENDER GRAPH ENTRY POINT ---
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Clear persistent batch collections from the previous frame to avoid GC allocs
            foreach (var list in batchedMatrices.Values) list.Clear();
            foreach (var list in batchedUVs.Values) list.Clear();
        
            bool hasAnyOutlines = false;

            foreach (var sprite in _cachedRenderers)
            {
                if (sprite == null || sprite.sprite == null || sprite.sprite.texture == null) continue;

                Sprite s = sprite.sprite;
                Texture2D tex = s.texture;

                // Ensure lists exist for this texture
                if (!batchedMatrices.ContainsKey(tex))
                {
                    // We allocate more space initially because each sprite adds 8 instances
                    batchedMatrices[tex] = new List<Matrix4x4>(1024);
                    batchedUVs[tex] = new List<Vector4>(1024);
                }

                // 1. Get exact size from the texture rect pixels
                Vector2 localSize = new Vector2(s.textureRect.width, s.textureRect.height) / s.pixelsPerUnit;

                // 2. Calculate the exact center offset of the texture relative to the pivot
                Vector2 pivotInTextureRect = s.pivot - s.textureRectOffset;
                Vector2 centerOffsetPixels = new Vector2(s.textureRect.width * 0.5f, s.textureRect.height * 0.5f) - pivotInTextureRect;
                Vector3 localCenterOffset = centerOffsetPixels / s.pixelsPerUnit;

                // 3. Support for SpriteRenderers using 'Sliced' or 'Tiled' draw modes
                if (sprite.drawMode != SpriteDrawMode.Simple)
                {
                    localSize = sprite.size; 
                    Vector2 normalizedPivot = new Vector2(s.pivot.x / s.rect.width, s.pivot.y / s.rect.height);
                    localCenterOffset = new Vector2(localSize.x * (0.5f - normalizedPivot.x), localSize.y * (0.5f - normalizedPivot.y));
                }

                // 4. Calculate the true world scale
                Vector3 lossyScale = sprite.transform.lossyScale;

                Vector3 finalScale = new Vector3(
                    lossyScale.x * localSize.x,
                    lossyScale.y * localSize.y,
                    lossyScale.z
                );

                // Account for SpriteRenderer flips
                if (sprite.flipX)
                {
                    finalScale.x *= -1f;
                    localCenterOffset.x *= -1f;
                }

                if (sprite.flipY)
                {
                    finalScale.y *= -1f;
                    localCenterOffset.y *= -1f;
                }

                // 5. Calculate world position applying the pivot offset
                Vector3 worldPivotOffset = sprite.transform.rotation * new Vector3(
                    localCenterOffset.x * lossyScale.x,
                    localCenterOffset.y * lossyScale.y,
                    localCenterOffset.z * lossyScale.z
                );

                Vector3 basePosition = sprite.transform.position + worldPivotOffset;
                Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(s);

                // 6. Generate 8 matrices per sprite and push to batch
                for (int i = 0; i < 8; i++)
                {
                    Vector3 offsetPosition = basePosition + _outlineOffsets[i];
                    Matrix4x4 matrix = Matrix4x4.TRS(offsetPosition, sprite.transform.rotation, finalScale);
                
                    batchedMatrices[tex].Add(matrix);
                    batchedUVs[tex].Add(uv);
                    hasAnyOutlines = true;
                }
            }

            if (!hasAnyOutlines) return;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Sprite Renderer Outline Pass", out var passData))
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                if (!resourceData.activeColorTexture.IsValid() || !resourceData.activeDepthTexture.IsValid())
                {
                    return;
                }

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);

                passData.mesh = _quadMesh;
                passData.material = _outlineMaterial;
                passData.sharedBlock = _propertyBlock;
                passData.chunks = new List<RenderChunk>();

                // Pre-process and chunk the data on the main thread
                foreach (var kvp in batchedMatrices)
                {
                    Texture2D tex = kvp.Key;
                    List<Matrix4x4> matricesList = kvp.Value;
                    List<Vector4> uvList = batchedUVs[tex];
                
                    int totalInstances = matricesList.Count;
                    if (totalInstances == 0) continue;

                    // Safely chunk into max blocks of 1023 instances. 
                    // With 8 outlines per sprite, lists will be larger, but this chunker handles it automatically.
                    for (int i = 0; i < totalInstances; i += MAX_INSTANC_PER_BLOCK)
                    {
                        int count = Mathf.Min(MAX_INSTANC_PER_BLOCK, totalInstances - i);
                        RenderChunk chunk = new RenderChunk
                        {
                            texture = tex,
                            matricesArray = new Matrix4x4[count],
                            uvsArray = new Vector4[count],
                            instanceCount = count
                        };
                    
                        matricesList.CopyTo(i, chunk.matricesArray, 0, count);
                        uvList.CopyTo(i, chunk.uvsArray, 0, count);
                    
                        passData.chunks.Add(chunk);
                    }
                }

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    foreach (var chunk in data.chunks)
                    {
                        data.sharedBlock.Clear();
        
                        data.sharedBlock.SetTexture(MainTex, chunk.texture);
                        data.sharedBlock.SetVectorArray(MainTexUVs, chunk.uvsArray);

                        context.cmd.DrawMeshInstanced(
                            data.mesh, 
                            0, 
                            data.material, 
                            0, 
                            chunk.matricesArray, 
                            chunk.instanceCount, 
                            data.sharedBlock
                        );
                    }
                });
            }
        }

        private class RenderChunk
        {
            public Texture2D texture;
            public Matrix4x4[] matricesArray;
            public Vector4[] uvsArray;
            public int instanceCount;
        }

        private class PassData
        {
            public Mesh mesh;
            public Material material;
            public MaterialPropertyBlock sharedBlock;
            public List<RenderChunk> chunks;
        }

        [System.Obsolete("Compatibility mode fallback only")]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }

        private Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0) };
            mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
            return mesh;
        }
    }
}