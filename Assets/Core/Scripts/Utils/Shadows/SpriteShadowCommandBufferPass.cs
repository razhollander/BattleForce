using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

public class SpriteShadowCommandBufferPass : ScriptableRenderPass
{
    private Material shadowMaterial;
    private Mesh quadMesh;
    
    // Kept as a persistent field to avoid garbage collection allocations every frame
    private MaterialPropertyBlock propertyBlock;
    
    private FixedOrderedList<Matrix4x4> matrices = new FixedOrderedList<Matrix4x4>(1023);
    private List<Vector4> uvOffsets = new List<Vector4>();
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
    
    public SpriteShadowCommandBufferPass(Material material, RenderPassEvent renderPassEvent)
    {
        this.shadowMaterial = material;
        this.renderPassEvent = renderPassEvent;
        this.quadMesh = CreateQuadMesh();
        this.propertyBlock = new MaterialPropertyBlock(); // Instantiated safely on the main thread
    }

    // --- UNITY 6 RENDER GRAPH ENTRY POINT ---
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        matrices.Clear();
        uvOffsets.Clear();
        
        Vector3 shadowOffset = new Vector3(0.0f, -0.0f, 0f);

        foreach (var sprite in _cachedRenderers)
        {
            if (sprite == null || sprite.sprite == null) continue;

            Sprite s = sprite.sprite;

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

            // 6. Assign the matrix and UVs
            ref var mat2 = ref matrices.AddAndGet();
            mat2.SetTRS(finalPosition, sprite.transform.rotation, finalScale);

            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(s);
            uvOffsets.Add(uv);
        }

        if (matrices.Count == 0) return;

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
            passData.instanceCount = matrices.Count;
            passData.sharedBlock = propertyBlock;

            // FIX: Manually copy the elements into a continuous, sequential array.
            // This prevents DrawMeshInstanced from reading stale, uninitialized data fields deep in RawArray.
            Matrix4x4[] continuousMatrices = new Matrix4x4[matrices.Count];
            for (int i = 0; i < matrices.Count; i++)
            {
                continuousMatrices[i] = matrices[i]; // Sequential structural indexing copy
            }
            passData.matricesArray = continuousMatrices;
            passData.uvsArray = uvOffsets.ToArray(); 

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                // Clear out stale parameters from the previous frame execution
                data.sharedBlock.Clear();
    
                // 1. Pass our custom Atlas UV arrays
                data.sharedBlock.SetVectorArray("_MainTex_UVs", data.uvsArray);

                // 2. FIXED: Grab the texture from our actual sprites and pass it to the shader
                if (_cachedRenderers.Count > 0 && _cachedRenderers[0] != null && _cachedRenderers[0].sprite != null)
                {
                    Texture2D spriteTexture = _cachedRenderers[0].sprite.texture;
                    data.sharedBlock.SetTexture("_MainTex", spriteTexture);
                }

                // Execute the draw call
                context.cmd.DrawMeshInstanced(
                    data.mesh, 
                    0, 
                    data.material, 
                    0, 
                    data.matricesArray, 
                    data.instanceCount, 
                    data.sharedBlock
                );
            });
        }
    }

    private class PassData
    {
        public Mesh mesh;
        public Material material;
        public Matrix4x4[] matricesArray;
        public Vector4[] uvsArray;
        public int instanceCount;
        public MaterialPropertyBlock sharedBlock;
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