using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchPlayerUIControllersView : MonoBehaviour
    {
        [field: SerializeField] public Transform PlayersContainer { get; private set; }
        [field: SerializeField] public MatchPlayerUIView PlayerUIView { get; private set; }
    }
}