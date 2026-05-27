using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.URPRenderFeatures.SpriteRendererSilhouetteRenderFeature.Scripts
{
    public class SpriteRendererSilhouetteRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material shadowMaterial;
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float outlineThickness;
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    
        private SpriteRendererShadowPass _rendererShadowPass;
        private SpriteRendererOutlinePass _rendererOutlinePass;
    
        // Called when the feature is first loaded, or when settings change in the inspector
        public override void Create()
        {
            _rendererShadowPass = new SpriteRendererShadowPass(shadowMaterial, _renderPassEvent);
            _rendererOutlinePass = new SpriteRendererOutlinePass(outlineMaterial, outlineThickness, _renderPassEvent);
        }

        // Called every frame per camera. This injects the pass into the renderer
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var isRenderingPreviewWindows = renderingData.cameraData.cameraType == CameraType.Preview;
            if (isRenderingPreviewWindows)
            {
                return;
            }

            renderer.EnqueuePass(_rendererShadowPass);
            renderer.EnqueuePass(_rendererOutlinePass);
        }
    }
}