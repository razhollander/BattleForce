using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerPowerUpHudView : MonoBehaviour
    {
        [SerializeField] private GameObject _container;
        [SerializeField] private Image _powerUpImage;

        public void SetPowerUp(bool isShown, Sprite powerUpSprite)
        {
            _container.SetActive(isShown);

            if (isShown)
            {
                _powerUpImage.sprite = powerUpSprite;
            }
        }
    }
}
