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
        
        Vector3 shadowOffset = new Vector3(0.2f, -0.2f, 0f);

        foreach (var sprite in _cachedRenderers)
        {
            if (sprite == null || sprite.sprite == null) continue;

            ref var mat2 = ref matrices.AddAndGet();
            mat2.SetTRS(sprite.transform.position + shadowOffset, sprite.transform.rotation, sprite.transform.lossyScale);
            
            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite.sprite);
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
                // FIX: Update the persistent property block instead of allocating a 'new' one on the execution loop thread
                data.sharedBlock.Clear();
                data.sharedBlock.SetVectorArray("_MainTex_UVs", data.uvsArray);

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