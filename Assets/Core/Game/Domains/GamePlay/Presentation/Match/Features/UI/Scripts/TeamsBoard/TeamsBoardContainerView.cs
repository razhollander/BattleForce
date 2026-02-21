using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamsBoardContainerView : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private TeamBoardUIView _teamBoardUIViewPrefab;
        
        private readonly Dictionary<ushort, TeamBoardUIView> _boardViewsPerTeam = new Dictionary<ushort, TeamBoardUIView>();

        public void CreateTeamBoard(ushort teamId, Color color, int teamGems, int teamBolts)
        {
            var teamView = Object.Instantiate(_teamBoardUIViewPrefab, _container);
            teamView.Setup(color, teamGems, teamBolts);
            _boardViewsPerTeam.Add(teamId, teamView);
        }

        public void UpdateTeamGems(ushort teamId, int gems)
        {
            _boardViewsPerTeam[teamId].UpdateGems(gems);
        }

        public void UpdateTeamBolts(ushort teamId, int teamBolts, CancellationTokenSource cancellationTokenSource)
        {
            _boardViewsPerTeam[teamId].UpdateBolts(teamBolts, cancellationTokenSource);
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
