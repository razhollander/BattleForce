// using System.Collections.Generic;
// using CoreDomain.Scripts.Services.Logger.Base;
// using Sirenix.Utilities;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.RenderGraphModule;
// using UnityEngine.Rendering.Universal;
//
// namespace Core.Scripts.Utils.Shadows
// {
//     public class SpriteShadowRenderFeature : ScriptableRendererFeature
//     {
//         [System.Serializable]
//         public class Settings
//         {
//             [Header("Shadow Settings")]
//             public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
//             public Vector2 shadowOffset = new Vector2(0.1f, -0.1f);
//
//             [Header("Outline Settings")]
//             public Color outlineColor = new Color(0f, 0f, 0f, 1f);
//             [Range(0.001f, 0.2f)] public float outlineSize = 0.02f;
//             
//             [Header("Pipeline Alignment")]
//             public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
//         }
//
//         public Settings settings = new Settings();
//         
//         private SpriteBackgroundPass _backgroundPass;
//         private SpriteShadowPass _shadowPass;
//         private SpriteOutlinePass _outlinePass;
//
//         public override void Create()
//         {
//             _backgroundPass = new SpriteBackgroundPass(settings);
//             _shadowPass = new SpriteShadowPass(settings);
//             _outlinePass = new SpriteOutlinePass(settings);
//         }
//
//         public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//         {
//             if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
//             {
//                 renderer.EnqueuePass(_backgroundPass);
//                 renderer.EnqueuePass(_shadowPass);
//                 renderer.EnqueuePass(_outlinePass);
//             }
//         }
//     }
//
//     // --- PASS 1: BACKGROUND RENDERER ---
//     public class SpriteBackgroundPass : ScriptableRenderPass
//     {
//         private readonly SpriteShadowRenderFeature.Settings _settings;
//         private static readonly List<Renderer> _cachedRenderers = new List<Renderer>();
//         private static bool _areBackgroundsDirty;
//
//         public static void RegisterRenderer(Renderer renderer)
//         {
//             _areBackgroundsDirty = true;
//             _cachedRenderers.Add(renderer);
//         }
//         
//         public static void UnregisterRenderer(Renderer renderer)
//         {
//             //_areBackgroundsDirty = true;
//             _cachedRenderers.Remove(renderer);
//         }
//
//         private class PassData
//         {
//             public List<Renderer> renderers;
//         }
//
//         public SpriteBackgroundPass(SpriteShadowRenderFeature.Settings settings)
//         {
//             _settings = settings;
//             renderPassEvent = settings.renderPassEvent;
//         }
//
//         private void RefreshRenderers()
//         {
//             if (!_areBackgroundsDirty || _cachedRenderers.IsNullOrEmpty())
//             {
//                 return;
//             }
//
//             _cachedRenderers.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
//             _areBackgroundsDirty = false;
//         }
//
//         public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
//         {
//             if (_cachedRenderers.Count == 0) return;
//             
//             RefreshRenderers();
//
//             using (var builder = renderGraph.AddRasterRenderPass<PassData>("Custom Sprite Background Pass", out var passData))
//             {
//                 passData.renderers = _cachedRenderers;
//
//                 UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
//                 builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
//                 builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);
//
//                 builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
//                 {
//                     foreach (var renderer in data.renderers)
//                     {
//                         if (renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled)
//                         {
//                             context.cmd.DrawRenderer(renderer, renderer.sharedMaterial, 0, 0);
//                         }
//                     }
//                 });
//             }
//         }
//     }
//
//     // --- PASS 2: SHADOW RENDERER ---
//     public class SpriteShadowPass : ScriptableRenderPass
//     {
//         private readonly SpriteShadowRenderFeature.Settings _settings;
//         private static readonly List<Renderer> _cachedRenderers = new List<Renderer>();
//         private static bool _areBackgroundsDirty;
//
//         public static void RegisterRenderer(Renderer renderer)
//         {
//             _areBackgroundsDirty = true;
//             _cachedRenderers.Add(renderer);
//         }
//         
//         public static void UnregisterRenderer(Renderer renderer)
//         {
//             //_areBackgroundsDirty = true;
//             _cachedRenderers.Remove(renderer);
//         }
//         
//         private Material _shadowMaterial;
//         
//         private static MaterialPropertyBlock _mpb;
//
//         private class PassData
//         {
//             public List<Renderer> renderers;
//             public Material material;
//             public Color color;
//             public Matrix4x4 shadowViewMatrix;
//             public Matrix4x4 shadowProjMatrix;
//             public Matrix4x4 originalViewMatrix;
//             public Matrix4x4 originalProjMatrix;
//         }
//
//         public SpriteShadowPass(SpriteShadowRenderFeature.Settings settings)
//         {
//             _settings = settings;
//             renderPassEvent = settings.renderPassEvent;
//
//             if (_shadowMaterial == null)
//             {
//                 Shader silhouetteShader = Shader.Find("Custom/SpriteSilhouette");
//                 _shadowMaterial = silhouetteShader != null ? new Material(silhouetteShader) : new Material(Shader.Find("Sprites/Default"));
//             }
//             
//             if (_mpb == null) _mpb = new MaterialPropertyBlock();
//         }
//
//         private void RefreshRenderers()
//         {
//             if (!_areBackgroundsDirty || _cachedRenderers.IsNullOrEmpty())
//             {
//                 return;
//             }
//
//             _cachedRenderers.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
//             _areBackgroundsDirty = false;
//         }
//
//         public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
//         {
//             if (_cachedRenderers.Count == 0)
//             {
//                 return;
//             }
//             
//             RefreshRenderers();
//
//             UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
//             Camera camera = cameraData.camera;
//
//             using (var builder = renderGraph.AddRasterRenderPass<PassData>("Custom Sprite Shadows Pass", out var passData))
//             {
//                 passData.renderers = _cachedRenderers;
//                 passData.material = _shadowMaterial;
//                 passData.color = _settings.shadowColor;
//
//                 passData.originalViewMatrix = camera.worldToCameraMatrix;
//                 passData.originalProjMatrix = camera.projectionMatrix;
//
//                 Matrix4x4 offsetMatrix = Matrix4x4.Translate(new Vector3(_settings.shadowOffset.x, _settings.shadowOffset.y, 0.01f));
//                 passData.shadowViewMatrix = camera.worldToCameraMatrix * offsetMatrix.inverse;
//                 passData.shadowProjMatrix = camera.projectionMatrix;
//
//                 UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
//                 builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
//                 builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);
//
//                 builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
//                 {
//                     if (data.material == null) return;
//
//                     data.material.color = data.color;
//                     context.cmd.SetViewProjectionMatrices(data.shadowViewMatrix, data.shadowProjMatrix);
//
//                     foreach (var renderer in data.renderers)
//                     {
//                         if (renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled)
//                         {
//                             if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
//                             {
//                                 Material sharedMat = renderer.sharedMaterial;
//                                 if (sharedMat != null)
//                                 {
//                                     Texture targetTex = sharedMat.HasProperty("_BaseMap") ? sharedMat.GetTexture("_BaseMap") : 
//                                                        sharedMat.HasProperty("_MainTex") ? sharedMat.GetTexture("_MainTex") : null;
//
//                                     if (targetTex != null)
//                                     {
//                                         _mpb.SetTexture("_MainTex", targetTex);
//                                         context.cmd.DrawRenderer(renderer, data.material, 0, 0);
//                                         continue;
//                                     }
//                                 }
//                             }
//
//                             context.cmd.DrawRenderer(renderer, data.material, 0, 0);
//                         }
//                     }
//
//                     context.cmd.SetViewProjectionMatrices(data.originalViewMatrix, data.originalProjMatrix);
//                 });
//             }
//         }
//     }
//
//     // --- PASS 3: OUTLINE RENDERER ---
//     public class SpriteOutlinePass : ScriptableRenderPass
//     {
//         private readonly SpriteShadowRenderFeature.Settings _settings;
//         private static readonly List<Renderer> _cachedRenderers = new List<Renderer>();
//         private static bool _areBackgroundsDirty;
//
//         public static void RegisterRenderer(Renderer renderer)
//         {
//             _areBackgroundsDirty = true;
//             _cachedRenderers.Add(renderer);
//         }
//         
//         public static void UnregisterRenderer(Renderer renderer)
//         {
//             //_areBackgroundsDirty = true;
//             _cachedRenderers.Remove(renderer);
//         }
//         
//         private Material _outlineMaterial;
//         
//         private static MaterialPropertyBlock _mpb;
//
//         private static readonly Vector2[] Directions = new Vector2[]
//         {
//             new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1),
//             new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized, 
//             new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
//         };
//
//         private class PassData
//         {
//             public List<Renderer> renderers;
//             public Material material;
//             public Color color;
//             public float size;
//             public Matrix4x4 viewMatrix;
//             public Matrix4x4 projMatrix;
//         }
//
//         public SpriteOutlinePass(SpriteShadowRenderFeature.Settings settings)
//         {
//             _settings = settings;
//             renderPassEvent = settings.renderPassEvent;
//
//             if (_outlineMaterial == null)
//             {
//                 Shader silhouetteShader = Shader.Find("Custom/SpriteSilhouette");
//                 _outlineMaterial = silhouetteShader != null ? new Material(silhouetteShader) : new Material(Shader.Find("Sprites/Default"));
//             }
//             
//             if (_mpb == null) _mpb = new MaterialPropertyBlock();
//         }
//
//         private void RefreshRenderers()
//         {
//             if (!_areBackgroundsDirty || _cachedRenderers.IsNullOrEmpty())
//             {
//                 return;
//             }
//
//             _cachedRenderers.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
//             _areBackgroundsDirty = false;
//         }
//
//         public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
//         {
//             if (_cachedRenderers.Count == 0) return;
//          
//             RefreshRenderers();
//
//             UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
//             Camera camera = cameraData.camera;
//
//             using (var builder = renderGraph.AddRasterRenderPass<PassData>("Custom Sprite Outlines Pass", out var passData))
//             {
//                 passData.renderers = _cachedRenderers;
//                 passData.material = _outlineMaterial;
//                 passData.color = _settings.outlineColor;
//                 passData.size = _settings.outlineSize;
//                 passData.viewMatrix = camera.worldToCameraMatrix;
//                 passData.projMatrix = camera.projectionMatrix;
//
//                 UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
//                 builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
//                 builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);
//
//                 builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
//                 {
//                     if (data.material == null) return;
//
//                     data.material.color = data.color;
//
//                     foreach (Vector2 dir in Directions)
//                     {
//                         Vector3 offset = new Vector3(dir.x * data.size, dir.y * data.size, 0.005f); 
//                         Matrix4x4 offsetMatrix = Matrix4x4.Translate(offset);
//                         
//                         context.cmd.SetViewProjectionMatrices(data.viewMatrix * offsetMatrix.inverse, data.projMatrix);
//
//                         foreach (var renderer in data.renderers)
//                         {
//                             /*if (renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled)
//                             {
//                                 if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
//                                 {*/
//                                     Material sharedMat = renderer.sharedMaterial;
//                                     if (sharedMat != null)
//                                     {
//                                         Texture targetTex = sharedMat.HasProperty("_BaseMap") ? sharedMat.GetTexture("_BaseMap") : 
//                                                            sharedMat.HasProperty("_MainTex") ? sharedMat.GetTexture("_MainTex") : null;
//
//                                         if (targetTex != null)
//                                         {
//                                             _mpb.SetTexture("_MainTex", targetTex);
//                                             context.cmd.DrawRenderer(renderer, data.material, 0, 0);
//                                             continue;
//                                         }
//                                     /*}
//                                 }*/
//
//                                 context.cmd.DrawRenderer(renderer, data.material, 0, 0);
//                             }
//                         }
//                     }
//
//                     context.cmd.SetViewProjectionMatrices(data.viewMatrix, data.projMatrix);
//                 });
//             }
//         }
//     }
// }