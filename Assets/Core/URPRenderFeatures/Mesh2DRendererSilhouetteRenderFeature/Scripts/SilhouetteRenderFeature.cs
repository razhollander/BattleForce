using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.URPRenderFeatures.Mesh2DRendererSilhouetteRenderFeature.Scripts
{
    public class SilhouetteRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class SilhouetteSettings
        {
            [Tooltip("The Layer containing the 2D opaque meshes to outline.")]
            public LayerMask layerMask = -1;

            [Tooltip("The override material using the Hidden/Custom/GlobalSilhouette shader")]
            public Material overrideMaterial;

            [Header("Shadow Settings")]
            public Color shadowColor = new Color(0, 0, 0, 0.5f);
            public Vector2 shadowOffset = new Vector2(0.1f, -0.1f);

            [Header("Outline Settings")]
            public Color outlineColor = Color.white;
            public float outlineWidth = 0.05f;
            public bool use8Directions = false;
        
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        public SilhouetteSettings settings = new SilhouetteSettings();
        private Mesh2DSilhouettePass _mesh2DSilhouettePass;

        public override void Create()
        {
            _mesh2DSilhouettePass = new Mesh2DSilhouettePass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_mesh2DSilhouettePass);
        }
    }
}