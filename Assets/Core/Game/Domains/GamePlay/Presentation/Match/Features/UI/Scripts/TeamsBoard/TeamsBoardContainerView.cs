using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamsBoardContainerView : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private TeamBoardUIView _teamBoardUIViewPrefab;
        
        private readonly Dictionary<ushort, TeamBoardUIView> _boardViewsPerTeam = new Dictionary<ushort, TeamBoardUIView>();

        public void CreateTeamBoard(ushort teamId, Color color)
        {
            var teamView = Object.Instantiate(_teamBoardUIViewPrefab, _container);
            teamView.Setup(color);
            _boardViewsPerTeam.Add(teamId, teamView);
        }

        public void UpdateTeamGems(ushort teamId, int gems)
        {
            _boardViewsPerTeam[teamId].UpdateGems(gems);
        }

        public void UpdateTeamBolts(ushort teamId, int bolts)
        {
            _boardViewsPerTeam[teamId].UpdateBolts(bolts);
        }

        public void DestroyAll()
        {
            foreach (var boardUIView in _boardViewsPerTeam)
            {
                Destroy(boardUIView.Value.gameObject);
            }
            
            _boardViewsPerTeam.Clear();
        }
    }
}
