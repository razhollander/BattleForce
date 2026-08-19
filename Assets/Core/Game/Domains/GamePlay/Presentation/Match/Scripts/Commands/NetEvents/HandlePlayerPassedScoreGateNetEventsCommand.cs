using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGainedEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePlayerPassedScoreGateNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchDataService _matchDataService;
        private IScoreGatesControllers _scoreGatesControllers;
        private IScoreGainedEffectController _scoreGainedEffectController;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IMatchPlayerUIControllers _playerUIControllers;
        private IAudioService _audioService;
        private PresentationGamePlayConfig _gamePlayConfig;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _scoreGatesControllers = _diContainer.Resolve<IScoreGatesControllers>();
            _scoreGainedEffectController = _diContainer.Resolve<IScoreGainedEffectController>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _gamePlayConfig = _diContainer.Resolve<PresentationGamePlayConfig>();
        }

        public void Execute()
        {
            var playerPassedScoreGateNetEvents = _cachedPresentationEventsService.PlayerPassedScoreGateNetEvents;

            if (playerPassedScoreGateNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var playerPassedScoreGateNetEvent in playerPassedScoreGateNetEvents)
            {
                var byTeamId = _matchDataService.GetPlayerTeamId(playerPassedScoreGateNetEvent.ByPlayerId);

                if (_scoreGatesControllers.TryGetScoreGatePosition(playerPassedScoreGateNetEvent.ScoreGateId, out var gatePosition))
                {
                    Color? outlineAndUnderlineColor = _gamePlayConfig.ColorPerTeamId.TryGetValue(byTeamId, out var teamColor)
                        ? teamColor
                        : null;
                    _scoreGainedEffectController.PlayEffect(playerPassedScoreGateNetEvent.ScoreGained, gatePosition, outlineAndUnderlineColor);
                }

                _scoreGatesControllers.SetTeamColor(playerPassedScoreGateNetEvent.ScoreGateId, byTeamId);
                _scoreGatesControllers.PlayScoreGatePassedAnimation(playerPassedScoreGateNetEvent.ScoreGateId);
                _scoreGatesControllers.SetScoreMultiplier(playerPassedScoreGateNetEvent.ScoreGateId, playerPassedScoreGateNetEvent.NextScoreMultiplier);
                _teamsBoardUIController.UpdateTeamGatePassScore(byTeamId, playerPassedScoreGateNetEvent.TeamBonusScoreTotal);
                _playerUIControllers.UpdatePlayerGatePassScore(playerPassedScoreGateNetEvent.ByPlayerId, playerPassedScoreGateNetEvent.ByPlayerBonusScoreTotal);
            }

            _audioService.PlayAudio(AudioClipType.ScoreGatePassed);
            playerPassedScoreGateNetEvents.Clear();
        }
    }
}
