using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Core.Scripts.Utils.Outlines
{
    public class SpriteOutlineRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Header("Layer Setup")]
            public LayerMask backgroundLayer;
            public LayerMask outlineLayers;

            [Header("Outline Settings")]
            public Color outlineColor = new Color(0f, 0f, 0f, 1f);
            [Range(0.001f, 0.2f)] public float outlineSize = 0.02f;
            
            [Header("Pipeline Alignment")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public Settings settings = new Settings();
        private SpriteOutlinePass _outlinePass;

        public override void Create()
        {
            _outlinePass = new SpriteOutlinePass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                renderer.EnqueuePass(_outlinePass);
            }
        }
    }
    public class SpriteOutlinePass : ScriptableRenderPass
    {
        private readonly SpriteOutlineRenderFeature.Settings _settings;
        private readonly List<Renderer> _cachedRenderers = new List<Renderer>();
        private Material _outlineMaterial;
        private float _lastRefreshTime;
        private const float RefreshInterval = 0.5f;
        
        private static MaterialPropertyBlock _mpb;

        // Precalculated 8-directional layout for a thick, merged structural outline
        private static readonly Vector2[] Directions = new Vector2[]
        {
            new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized, 
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
        };

        private class PassData
        {
            public List<Renderer> renderers;
            public Material material;
            public Color color;
            public float size;
            public Matrix4x4 viewMatrix;
            public Matrix4x4 projMatrix;
        }

        public SpriteOutlinePass(SpriteOutlineRenderFeature.Settings settings)
        {
            _settings = settings;
            renderPassEvent = settings.renderPassEvent;

            if (_outlineMaterial == null)
            {
                // Reusing your silhouette shader setup
                Shader silhouetteShader = Shader.Find("Custom/SpriteSilhouette");
                _outlineMaterial = silhouetteShader != null ? new Material(silhouetteShader) : new Material(Shader.Find("Sprites/Default"));
            }
            
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
        }

        private void RefreshRenderers()
        {
            _cachedRenderers.Clear();
            var allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var renderer in allRenderers)
            {
                if (((1 << renderer.gameObject.layer) & _settings.outlineLayers) != 0)
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

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            Camera camera = cameraData.camera;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Custom Sprite Outlines Pass", out var passData))
            {
                passData.renderers = _cachedRenderers;
                passData.material = _outlineMaterial;
                passData.color = _settings.outlineColor;
                passData.size = _settings.outlineSize;
                passData.viewMatrix = camera.worldToCameraMatrix;
                passData.projMatrix = camera.projectionMatrix;

                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (data.material == null) return;

                    data.material.color = data.color;

                    // 1. Loop through directions first to MERGE the silhouette blocks seamlessly
                    foreach (Vector2 dir in Directions)
                    {
                        Vector3 offset = new Vector3(dir.x * data.size, dir.y * data.size, 0.005f); 
                        Matrix4x4 offsetMatrix = Matrix4x4.Translate(offset);
                        
                        // Shift the camera viewing matrices slightly per direction pass
                        context.cmd.SetViewProjectionMatrices(data.viewMatrix * offsetMatrix.inverse, data.projMatrix);

                        // 2. Render all silhouettes back-to-back for this directional step
                        foreach (var renderer in data.renderers)
                        {
                            if (renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled)
                            {
                                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                                {
                                    Material sharedMat = renderer.sharedMaterial;
                                    if (sharedMat != null)
                                    {
                                        Texture targetTex = sharedMat.HasProperty("_BaseMap") ? sharedMat.GetTexture("_BaseMap") : 
                                                           sharedMat.HasProperty("_MainTex") ? sharedMat.GetTexture("_MainTex") : null;

                                        if (targetTex != null)
                                        {
                                            _mpb.SetTexture("_MainTex", targetTex);
                                            context.cmd.DrawRenderer(renderer, data.material, 0, 0);
                                            continue;
                                        }
                                    }
                                }

                                context.cmd.DrawRenderer(renderer, data.material, 0, 0);
                            }
                        }
                    }

                    // Reset to standard camera projection matrices
                    context.cmd.SetViewProjectionMatrices(data.viewMatrix, data.projMatrix);
                });
            }
        }
    }
}