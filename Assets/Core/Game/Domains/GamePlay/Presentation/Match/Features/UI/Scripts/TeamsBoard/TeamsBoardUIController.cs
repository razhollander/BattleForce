using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
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
        private readonly IStageCancellationTokenProvider _stageCancellationTokenProvider;

        public TeamsBoardUIController(TeamsBoardContainerView view, PresentationGamePlayConfig presentationGamePlayConfig, IStageCancellationTokenProvider stageCancellationTokenProvider)
        {
            _view = view;
            _presentationGamePlayConfig = presentationGamePlayConfig;
            _stageCancellationTokenProvider = stageCancellationTokenProvider;
        }

        public void UpdateTeamGems(ushort teamId, int gems)
        {
            _view.UpdateTeamGems(teamId, gems);
        }

        public void UpdateTeamBolts(ushort teamId, int teamBolts)
        {
            _view.UpdateTeamBolts(teamId, teamBolts, _stageCancellationTokenProvider.CancellationTokenSource);
        }

        public void UpdateTeamMolesHit(ushort teamId, int molesHit)
        {
            _view.UpdateTeamMolesHit(teamId, molesHit);
        }

        public void SetIsMolesHitShown(bool isShown)
        {
            _view.SetIsMolesHitShown(isShown);
        }

        public void SetIsGatePassScoreShown(bool isShown)
        {
            _view.SetIsGatePassScoreShown(isShown);
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
