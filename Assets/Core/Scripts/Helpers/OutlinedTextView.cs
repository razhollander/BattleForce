using TMPro;
using UnityEngine;

namespace Core.Scripts.Helpers
{
    public class OutlinedTextView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro[] _texts;
        [SerializeField] private TextMeshPro _outlineText;
        [SerializeField] private TextMeshPro _underlineText;

        // Keeps the outline's current alpha so callers can recolour the outline without fighting the fade animations that
        // drive alpha through SetAlpha.
        public Color OutlineColor
        {
            get => _outlineText.color;
            set => _outlineText.color = new Color(value.r, value.g, value.b, _outlineText.color.a);
        }

        // Same alpha-preserving recolour as OutlineColor, for the underline layer behind the text.
        public Color UnderlineColor
        {
            get => _underlineText.color;
            set => _underlineText.color = new Color(value.r, value.g, value.b, _underlineText.color.a);
        }

        public void SetText(string text)
        {
            foreach (var textMeshPro in _texts)
            {
                textMeshPro.text = text;
            }
        }
        
        public void SetAlpha(float alpha)
        {
            foreach (var textMeshPro in _texts)
            {
                textMeshPro.color = new Color(textMeshPro.color.r, textMeshPro.color.g, textMeshPro.color.b, alpha);
            }
        }
    }
}
