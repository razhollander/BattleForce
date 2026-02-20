using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamsBoardUIController : ITeamsBoardUIController
    {
        private readonly TeamsBoardContainerView _view;
        private readonly PresentationGamePlayConfig _presentationGamePlayConfig;
        private readonly IStateMachineService _stateMachineService;

        public TeamsBoardUIController(TeamsBoardContainerView view, PresentationGamePlayConfig presentationGamePlayConfig, IStateMachineService stateMachineService)
        {
            _view = view;
            _presentationGamePlayConfig = presentationGamePlayConfig;
            _stateMachineService = stateMachineService;
        }

        public void UpdateTeamGems(ushort teamId, int gems)
        {
            _view.UpdateTeamGems(teamId, gems);
        }

        public void UpdateTeamBolts(ushort teamId, int bolts)
        {
            _view.UpdateTeamBolts(teamId, bolts, _stateMachineService.CurrentState().CancellationTokenSource);
        }

        public void CreateTeamBoard(ushort teamId, int teamGems, int teamBolts)
        {
            if (_presentationGamePlayConfig.ColorPerTeamId.TryGetValue(teamId, out var color))
            {
                _view.CreateTeamBoard(teamId, color, teamGems, teamBolts);
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
