using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.LoadingRing
{
    public class PlayerLoadingRingView : MonoBehaviour
    {
        private const int ArcEmptyValue = 180;
        private const int ArcFullValue = 0;
        private static readonly int ARC_SHADER_PROPERTY = Shader.PropertyToID("_HalfArc");
        private static readonly int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_Color");
        private static readonly int THICKNESS_SHADER_PROPERTY = Shader.PropertyToID("_Thickness");
        
        [SerializeField] private Color _talentFullColor = Color.yellow;
        [SerializeField] private Color _talentEmptyColor = Color.white;
        [SerializeField] private float _talentFullThickness = 0.09f;
        [SerializeField] private float _talentEmptyThickness = 0.062f;
        [SerializeField] private SpriteRenderer _spriteRenderer;
       
        private Material _material;

        public void OnCreated()
        {
            _material = _spriteRenderer.material;
        }

        public void SetRingArc(float cooldownLeft, float maxCooldown)
        {
            var ratio = Mathf.Clamp01(cooldownLeft / maxCooldown);
            var arcValue = Mathf.RoundToInt(Mathf.Lerp(ArcFullValue, ArcEmptyValue, ratio));

            _material.SetFloat(ARC_SHADER_PROPERTY, arcValue);

            var isNoCooldown = Mathf.Approximately(cooldownLeft, 0);

            if (isNoCooldown)
            {
                _material.SetFloat(THICKNESS_SHADER_PROPERTY, _talentFullThickness);
                _material.SetColor(COLOR_SHADER_PROPERTY, _talentFullColor);
            }
            else
            {
                _material.SetFloat(THICKNESS_SHADER_PROPERTY, _talentEmptyThickness);
                _material.SetColor(COLOR_SHADER_PROPERTY, _talentEmptyColor);
            }
        }


        public void SetRingScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }
    }
}
