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

        public TeamsBoardUIController(TeamsBoardContainerView view, PresentationGamePlayConfig presentationGamePlayConfig, IMatchDataService matchDataService)
        {
            _view = view;
            _presentationGamePlayConfig = presentationGamePlayConfig;
            _matchDataService = matchDataService;
        }

        public void UpdateTeamGems(ushort teamId, int gems)
        {
            _view.UpdateTeamGems(teamId, gems);
        }

        public void UpdateTeamBolts(ushort teamId, int bolts)
        {
            _view.UpdateTeamBolts(teamId, bolts);
        }

        public void CreateTeamBoard(ushort teamId)
        {
            if (_presentationGamePlayConfig.ColorPerTeamId.TryGetValue(teamId, out var color))
            {
                _view.CreateTeamBoard(teamId, color);
            }
            else
            {
                LogService.LogError($"No color for team with id {teamId}");
            }
        }

        public void DestroyAll()
        {
            _view.DestroyAll();
        }
    }
}
