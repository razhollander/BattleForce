using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamsBoardContainerView : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private TeamBoardUIView _teamBoardUIViewPrefab;

        public Transform Container => _container;
        public TeamBoardUIView Prefab => _teamBoardUIViewPrefab;
    }
}
