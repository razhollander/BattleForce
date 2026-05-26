using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.Scripts.Utils.Shadows
{
    public class SpriteBackgroundRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Header("Pipeline Alignment")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public Settings settings = new Settings();
        private SpriteBackgroundPass _backgroundPass;

        public override void Create()
        {
            _backgroundPass = new SpriteBackgroundPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                renderer.EnqueuePass(_backgroundPass);
            }
        }
    }

    // --- PASS: BACKGROUND RENDERER ---
    public class SpriteBackgroundPass : ScriptableRenderPass
    {
        private static readonly List<Renderer> _cachedRenderers = new List<Renderer>();
        private static bool _areBackgroundsDirty;

        public static void RegisterRenderer(Renderer renderer)
        {
            _areBackgroundsDirty = true;
            _cachedRenderers.Add(renderer);
        }
        
        public static void UnregisterRenderer(Renderer renderer)
        {
            _cachedRenderers.Remove(renderer);
        }

        private class PassData
        {
            public List<Renderer> renderers;
            public RendererListHandle rendererListHandle;
        }

        public SpriteBackgroundPass(SpriteBackgroundRenderFeature.Settings settings)
        {
            renderPassEvent = settings.renderPassEvent;
        }

        private void RefreshRenderers()
        {
            if (!_areBackgroundsDirty || _cachedRenderers.IsNullOrEmpty())
            {
                return;
            }

            _cachedRenderers.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
            _areBackgroundsDirty = false;
        }

        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Draw Background", out var passData))
            {
                // Target the layer(s) your planes exist on
                FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all, LayerMask.GetMask("Background"));
        
                // --- THIS IS THE CRITICAL PART FOR BATCHING ---
                // OptimizeStateChanges tells Unity to group the draw calls by Material/Shader 
                // before sorting by depth, ensuring your materials stay nicely batched.
                SortingCriteria sorting = SortingCriteria.OptimizeStateChanges; 
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                SortingSettings sortSettings = new SortingSettings(cameraData.camera) {criteria = sorting};

                DrawingSettings drawSettings = new DrawingSettings(new ShaderTagId("Universal2D"), sortSettings);
                
                // DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(
                //     new ShaderTagId("Universal2D"), // Standard URP 2D pass
                //     ref renderingData, 
                //     sorting
                // );

                // Request the RendererList
                RendererListParams listParams = new RendererListParams(
                    renderingData.cullResults, 
                    drawSettings, 
                    filterSettings
                );

                passData.rendererListHandle = renderGraph.CreateRendererList(listParams);
                builder.UseRendererList(passData.rendererListHandle);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.rendererListHandle);
                });
            }
        }
    }
}

