using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Views
{
    public class StartMatchButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        public void Setup(Vector2 position, float radius)
        {
            transform.position = position;
            SetRadius(radius);
        }

        public void SetText(string text)
        {
            _text.text = text;
        }
        
        private void SetRadius(float radius)
        {
            transform.localScale = new Vector3(radius * 2, radius * 2, 1);
        }
    }
}
