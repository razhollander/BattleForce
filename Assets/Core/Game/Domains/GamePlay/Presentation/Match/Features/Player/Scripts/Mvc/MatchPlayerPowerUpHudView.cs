using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerPowerUpHudView : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private Image _powerUpImage;

        public void SetPowerUp(bool hasPowerUp, Sprite icon)
        {
            if (_container != null)
            {
                _container.SetActive(hasPowerUp);
            }

            if (hasPowerUp && _powerUpImage != null)
            {
                _powerUpImage.sprite = icon;
            }
        }
    }
}
