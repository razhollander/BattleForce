using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamsBoardUIController : ITeamsBoardUIController
    {
        private readonly TeamsBoardContainerView _view;
        private readonly PresentationGamePlayConfig _presentationGamePlayConfig;
        private readonly IMatchDataService _matchDataService;
        private readonly Dictionary<ushort, TeamBoardUIView> _boardViewsPerTeam = new Dictionary<ushort, TeamBoardUIView>();

        public TeamsBoardUIController(TeamsBoardContainerView view, PresentationGamePlayConfig presentationGamePlayConfig, IMatchDataService matchDataService)
        {
            _view = view;
            _presentationGamePlayConfig = presentationGamePlayConfig;
            _matchDataService = matchDataService;
        }

        public void InitEntryPoint()
        {
            foreach (ushort teamId in _matchDataService.TeamIds)
            {
                var teamView = Object.Instantiate(_view.Prefab, _view.Container);
                if (_presentationGamePlayConfig.ColorPerTeamId.TryGetValue(teamId, out var color))
                {
                    teamView.Setup(color);
                }
                else
                {
                    LogService.LogError($"No color for team with id {teamId}");
                }
                _boardViewsPerTeam.Add(teamId, teamView);
            }
        }

        public void UpdateTeamGems(ushort teamId, int gems)
        {
            _boardViewsPerTeam[teamId].UpdateGems(gems);
        }
    }
}
