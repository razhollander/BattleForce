using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

public class SpriteShadowPass : ScriptableRenderPass
{
    private Material shadowMaterial;
    private Mesh quadMesh;
    
    // Kept as a persistent field to avoid garbage collection allocations every frame
    private MaterialPropertyBlock propertyBlock;
    
    // Use dictionaries to group by texture, preventing the "circle shadow" bug
    private Dictionary<Texture2D, List<Matrix4x4>> batchedMatrices = new Dictionary<Texture2D, List<Matrix4x4>>();
    private Dictionary<Texture2D, List<Vector4>> batchedUVs = new Dictionary<Texture2D, List<Vector4>>();
    
    private static readonly List<SpriteRenderer> _cachedRenderers = new List<SpriteRenderer>();
    
    public static void RegisterRenderer(SpriteRenderer renderer)
    {
        _cachedRenderers.Add(renderer);
        _cachedRenderers.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
    }
        
    public static void UnregisterRenderer(SpriteRenderer renderer)
    {
        _cachedRenderers.Remove(renderer);
    }
    
    public SpriteShadowPass(Material material, RenderPassEvent renderPassEvent)
    {
        this.shadowMaterial = material;
        this.renderPassEvent = renderPassEvent;
        this.quadMesh = CreateQuadMesh();
        this.propertyBlock = new MaterialPropertyBlock(); // Instantiated safely on the main thread
    }

    // --- UNITY 6 RENDER GRAPH ENTRY POINT ---
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // Clear persistent batch collections from the previous frame to avoid GC allocs
        foreach (var list in batchedMatrices.Values) list.Clear();
        foreach (var list in batchedUVs.Values) list.Clear();
        
        Vector3 shadowOffset = new Vector3(-0.3f, -0.5f, 0f);
        bool hasAnyShadows = false;

        foreach (var sprite in _cachedRenderers)
        {
            if (sprite == null || sprite.sprite == null || sprite.sprite.texture == null) continue;

            Sprite s = sprite.sprite;
            Texture2D tex = s.texture;

            // Ensure lists exist for this texture
            if (!batchedMatrices.ContainsKey(tex))
            {
                batchedMatrices[tex] = new List<Matrix4x4>(1023);
                batchedUVs[tex] = new List<Vector4>(1023);
            }

            // 1. Get exact size from the texture rect pixels (ignores custom mesh outlines that squish the image)
            Vector2 localSize = new Vector2(s.textureRect.width, s.textureRect.height) / s.pixelsPerUnit;

            // 2. Calculate the exact center offset of the texture relative to the pivot
            Vector2 pivotInTextureRect = s.pivot - s.textureRectOffset;
            Vector2 centerOffsetPixels = new Vector2(s.textureRect.width * 0.5f, s.textureRect.height * 0.5f) - pivotInTextureRect;
            Vector3 localCenterOffset = centerOffsetPixels / s.pixelsPerUnit;

            // 3. Support for SpriteRenderers using 'Sliced' or 'Tiled' draw modes
            if (sprite.drawMode != SpriteDrawMode.Simple)
            {
                localSize = sprite.size; // Sliced mode ignores texture size and uses the renderer size
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

            // Account for SpriteRenderer flips (this must flip both the scale AND the pivot offset)
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

            Vector3 finalPosition = sprite.transform.position + worldPivotOffset + shadowOffset;

            // 6. Assign the matrix and UVs to the specific texture batch
            Matrix4x4 matrix = Matrix4x4.TRS(finalPosition, sprite.transform.rotation, finalScale);
            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(s);
            
            batchedMatrices[tex].Add(matrix);
            batchedUVs[tex].Add(uv);
            hasAnyShadows = true;
        }

        if (!hasAnyShadows) return;

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("CustomSpriteShadowPass", out var passData))
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (!resourceData.activeColorTexture.IsValid() || !resourceData.activeDepthTexture.IsValid())
            {
                return;
            }

            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);

            passData.mesh = quadMesh;
            passData.material = shadowMaterial;
            passData.sharedBlock = propertyBlock;
            passData.chunks = new List<RenderChunk>();

            // Pre-process and chunk the data on the main thread so the Render Func is allocation-free
            foreach (var kvp in batchedMatrices)
            {
                Texture2D tex = kvp.Key;
                List<Matrix4x4> matricesList = kvp.Value;
                List<Vector4> uvList = batchedUVs[tex];
                
                int totalInstances = matricesList.Count;
                if (totalInstances == 0) continue;

                // Chunk into max blocks of 1023 instances
                for (int i = 0; i < totalInstances; i += 1023)
                {
                    int count = Mathf.Min(1023, totalInstances - i);
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
                    // Clear out stale parameters from the previous execution
                    data.sharedBlock.Clear();
        
                    // 1. Pass the correct texture for this chunk
                    data.sharedBlock.SetTexture("_MainTex", chunk.texture);
                    
                    // 2. Pass our custom Atlas UV arrays
                    data.sharedBlock.SetVectorArray("_MainTex_UVs", chunk.uvsArray);

                    // Execute the draw call for this specific chunk
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