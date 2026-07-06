using TMPro;
using UnityEngine;

namespace Core.Scripts.Helpers
{
    public class OutlinedTextView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro[] _texts;

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
