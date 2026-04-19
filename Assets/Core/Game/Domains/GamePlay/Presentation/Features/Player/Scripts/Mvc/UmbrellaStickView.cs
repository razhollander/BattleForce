using UnityEngine;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc
{
    public class UmbrellaStickView : MonoBehaviour
    {
        [SerializeField] private Transform _umbrellaParent;
        
        public void ShowUmbrella()
        {
            _umbrellaParent.gameObject.TrySetActive(true);
        }

        public void HideUmbrella()
        {
            _umbrellaParent.gameObject.TrySetActive(false);
        }

        public void SetRotation(Quaternion rotation)
        {
            _umbrellaParent.rotation = rotation;
        }
    }
}