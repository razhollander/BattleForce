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
        private readonly SpriteBackgroundRenderFeature.Settings _settings;
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
        }

        public SpriteBackgroundPass(SpriteBackgroundRenderFeature.Settings settings)
        {
            _settings = settings;
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
            if (_cachedRenderers.Count == 0) return;
            
            RefreshRenderers();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Custom Sprite Background Pass", out var passData))
            {
                passData.renderers = _cachedRenderers;

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    foreach (var renderer in data.renderers)
                    {
                        if (renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled)
                        {
                            context.cmd.DrawRenderer(renderer, renderer.sharedMaterial, 0, 0);
                        }
                    }
                });
            }
        }
    }
}