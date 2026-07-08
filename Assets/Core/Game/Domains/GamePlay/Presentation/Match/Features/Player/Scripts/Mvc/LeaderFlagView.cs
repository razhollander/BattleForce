using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class LeaderFlagView : MonoBehaviour
    {
        [SerializeField] private GameObject _flagRoot;
        [SerializeField] private GameObject _rightFlag;
        [SerializeField] private GameObject _leftFlag;

        public void SetIsShown(bool isShown)
        {
            _flagRoot.SetActive(isShown);
        }

        public void SetIsRight(bool isRight)
        {
            _rightFlag.SetActive(isRight);
            _leftFlag.SetActive(!isRight);
        }
    }
}
