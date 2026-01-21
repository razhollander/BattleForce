using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchUIView : MonoBehaviour
    {
        [SerializeField] private Transform _playersContainer;
        public Transform PlayersContainer => _playersContainer;
    }
}