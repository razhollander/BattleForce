using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.Scripts.Utils.Shadows
{
    public class SpriteShadowRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
            public Vector2 shadowOffset = new Vector2(0.1f, -0.1f);
            public LayerMask shadowLayers;
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public Settings settings = new Settings();
        private SpriteShadowPass _shadowPass;

        public override void Create()
        {
            _shadowPass = new SpriteShadowPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Only render for designated game cameras
            if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                renderer.EnqueuePass(_shadowPass);
            }
        }
    }

    public class SpriteShadowPass : ScriptableRenderPass
    {
        private readonly SpriteShadowRenderFeature.Settings _settings;
        private readonly List<SpriteRenderer> _cachedRenderers = new List<SpriteRenderer>();
        private Material _shadowMaterial;
        private float _lastRefreshTime;
        private const float RefreshInterval = 0.5f;

        // Class to pass data safely into the Render Graph context
        private class PassData
        {
            public List<SpriteRenderer> renderers;
            public Material material;
            public Color color;
            public Matrix4x4 shadowViewMatrix;
            public Matrix4x4 shadowProjMatrix;
            public Matrix4x4 originalViewMatrix;
            public Matrix4x4 originalProjMatrix;
        }

        public SpriteShadowPass(SpriteShadowRenderFeature.Settings settings)
        {
            _settings = settings;
            renderPassEvent = settings.renderPassEvent;

            if (_shadowMaterial == null)
            {
                // Swap out the default shader for your new silhouette shader
                Shader silhouetteShader = Shader.Find("Custom/SpriteSilhouette");
                if (silhouetteShader != null)
                {
                    _shadowMaterial = new Material(silhouetteShader);
                }
                else
                {
                    Debug.LogError("Could not find 'Custom/SpriteSilhouette' shader. Falling back to default.");
                    _shadowMaterial = new Material(Shader.Find("Sprites/Default"));
                }
            }
        }

        private void RefreshRenderers()
        {
            _cachedRenderers.Clear();
            var allRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

            foreach (var renderer in allRenderers)
            {
                if (((1 << renderer.gameObject.layer) & _settings.shadowLayers) != 0)
                {
                    _cachedRenderers.Add(renderer);
                }
            }

            _lastRefreshTime = Time.time;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (Time.time - _lastRefreshTime > RefreshInterval || _cachedRenderers.Count == 0)
            {
                RefreshRenderers();
            }

            if (_cachedRenderers.Count == 0) return;
            Debug.Log("_cachedRenderers.Count: "+_cachedRenderers.Count);
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            Camera camera = cameraData.camera;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Custom Sprite Shadows Pass", out var passData))
            {
                passData.renderers = _cachedRenderers;
                passData.material = _shadowMaterial;
                passData.color = _settings.shadowColor;

                // Cache original matrices to restore them later
                passData.originalViewMatrix = camera.worldToCameraMatrix;
                passData.originalProjMatrix = camera.projectionMatrix;

                // Calculate shadow matrices
                Matrix4x4 offsetMatrix = Matrix4x4.Translate(new Vector3(_settings.shadowOffset.x, _settings.shadowOffset.y, 0.01f));
                passData.shadowViewMatrix = camera.worldToCameraMatrix * offsetMatrix.inverse;
                passData.shadowProjMatrix = camera.projectionMatrix; // Keeping projection the same, but you can modify this if needed

                // Configure graph resource hooks
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (data.material == null) return;

                    data.material.color = data.color;

                    // Apply the custom View and Projection matrices directly to the raw command buffer
                    context.cmd.SetViewProjectionMatrices(data.shadowViewMatrix, data.shadowProjMatrix);

                    foreach (var renderer in data.renderers)
                    {
                        if (renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled)
                        {
                            context.cmd.DrawRenderer(renderer, data.material, 0, 0);
                        }
                    }

                    // CRITICAL: Revert back to the camera's original matrices so subsequent passes aren't broken
                    context.cmd.SetViewProjectionMatrices(data.originalViewMatrix, data.originalProjMatrix);
                });
            }
        }
    }
}