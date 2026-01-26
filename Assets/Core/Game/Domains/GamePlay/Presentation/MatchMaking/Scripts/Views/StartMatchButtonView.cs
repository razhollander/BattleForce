using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Views
{
    public class StartMatchButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Transform _visualRoot;

        public void SetText(string text)
        {
            if (_text != null)
            {
                _text.text = text;
            }
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public void SetRadius(float radius)
        {
            if (_visualRoot != null)
            {
                _visualRoot.localScale = new Vector3(radius * 2, radius * 2, 1);
            }
        }
    }
}
