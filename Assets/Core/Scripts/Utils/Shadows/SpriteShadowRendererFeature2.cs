using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpriteShadowRendererFeature2 : ScriptableRendererFeature
{
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineThickness;
    [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    
    private SpriteShadowPass _shadowPass;
    private SpriteOutlinePass _outlinePass;
    
    // Called when the feature is first loaded, or when settings change in the inspector
    public override void Create()
    {
        if (shadowMaterial != null)
        {
            _shadowPass = new SpriteShadowPass(shadowMaterial, _renderPassEvent);
        }
        
        if (shadowMaterial != null)
        {
            _outlinePass = new SpriteOutlinePass(outlineMaterial, outlineThickness, _renderPassEvent);
        }
    }

    // Called every frame per camera. This injects the pass into the renderer
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Don't render shadows if the material is missing or we are rendering the preview windows
        if (_shadowPass == null || renderingData.cameraData.cameraType == CameraType.Preview)
        {
            return;
        }

        renderer.EnqueuePass(_shadowPass);
        renderer.EnqueuePass(_outlinePass);
    }
}