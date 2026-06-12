using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.URPRenderFeatures.Mesh2DRendererSilhouetteRenderFeature.Scripts
{
    public class Mesh2DSilhouettePass : ScriptableRenderPass
    {
        private const string SILHOUETTE_OFFSET_FIELD_NAME = "_GlobalSilhouetteOffset";
        private const string SILHOUETTE_COLOR_FIELD_NAME = "_GlobalSilhouetteColor";
            
        private SilhouetteRenderFeature.SilhouetteSettings _settings;
        private Vector2[] _outlineDirections;

        private class PassData
        {
            // We now store a distinct handle for every draw call
            public RendererListHandle shadowRendererList;
            public RendererListHandle[] outlineRendererLists;

            public Color shadowColor;
            public Vector2 shadowOffset;

            public Color outlineColor;
            public float outlineWidth;
            public Vector2[] outlineDirections;
        }

        public Mesh2DSilhouettePass(SilhouetteRenderFeature.SilhouetteSettings settings)
        {
            _settings = settings;
            renderPassEvent = settings.renderPassEvent;

            // Pre-cache directions
            if (settings.use8Directions)
            {
                _outlineDirections = new Vector2[]
                {
                    new Vector2(0, 1), new Vector2(0.707f, 0.707f), new Vector2(1, 0), new Vector2(0.707f, -0.707f),
                    new Vector2(0, -1), new Vector2(-0.707f, -0.707f), new Vector2(-1, 0), new Vector2(-0.707f, 0.707f)
                };
            }
            else
            {
                _outlineDirections = new Vector2[] {Vector2.up, Vector2.down, Vector2.left, Vector2.right};
            }
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            SortingSettings sortSettings = new SortingSettings(cameraData.camera) {criteria = SortingCriteria.CommonTransparent};
            DrawingSettings drawSettings = new DrawingSettings(new ShaderTagId("Universal2D"), sortSettings);
            drawSettings.SetShaderPassName(1, new ShaderTagId("UniversalForward"));
            drawSettings.SetShaderPassName(2, new ShaderTagId("SRPDefaultUnlit"));

            drawSettings.overrideMaterial = _settings.overrideMaterial;
            drawSettings.overrideMaterialPassIndex = 0;

            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all, _settings.layerMask);

            // Create the base parameters for the renderer list
            RendererListParams rlParams = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("2D Mesh Effects Pass", out var passData))
            {
                passData.shadowColor = _settings.shadowColor;
                passData.shadowOffset = _settings.shadowOffset;
                passData.outlineColor = _settings.outlineColor;
                passData.outlineWidth = _settings.outlineWidth;
                passData.outlineDirections = _outlineDirections;

                // --- THE FIX: Create a distinct RendererList for the Shadow ---
                passData.shadowRendererList = renderGraph.CreateRendererList(rlParams);
                builder.UseRendererList(passData.shadowRendererList);

                var shouldInitRendererListHandleArray = passData.outlineRendererLists == null || passData.outlineRendererLists.Length != _outlineDirections.Length;
                if (shouldInitRendererListHandleArray)
                {
                    passData.outlineRendererLists = new RendererListHandle[_outlineDirections.Length];
                }

                for (int i = 0; i < _outlineDirections.Length; i++)
                {
                    passData.outlineRendererLists[i] = renderGraph.CreateRendererList(rlParams);
                    builder.UseRendererList(passData.outlineRendererLists[i]);
                }

                var resourceData = frameData.Get<UniversalResourceData>();
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    // --- DRAW SHADOW ---
                    context.cmd.SetGlobalVector(SILHOUETTE_OFFSET_FIELD_NAME, new Vector4(data.shadowOffset.x, data.shadowOffset.y, 0.01f, 0));
                    context.cmd.SetGlobalColor(SILHOUETTE_COLOR_FIELD_NAME, data.shadowColor);

                    // Consume the shadow handle
                    context.cmd.DrawRendererList(data.shadowRendererList);

                    // --- DRAW OUTLINES ---
                    context.cmd.SetGlobalColor(SILHOUETTE_COLOR_FIELD_NAME, data.outlineColor);

                    for (var i = 0; i < data.outlineDirections.Length; i++)
                    {
                        var offset = new Vector3(data.outlineDirections[i].x * data.outlineWidth, data.outlineDirections[i].y * data.outlineWidth, 0.005f);
                        context.cmd.SetGlobalVector(SILHOUETTE_OFFSET_FIELD_NAME, offset);

                        // Consume a unique outline handle for each pass
                        context.cmd.DrawRendererList(data.outlineRendererLists[i]);
                    }
                });
            }
        }
    }
}