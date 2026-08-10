using Core.Game.Domains.GamePlay.Presentation.Match.Features.MoleHitScoreEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    // GatePass counterpart of HandleMoleHitNetEventsCommand: pops the "+1" from the gate, tints the gate to the scoring
    // team's colour, and updates the top-middle team board and the scoring player's UI. Score model mutations already
    // happened in PresentationMatchNetEventsHandler.
    public class HandleScoreGatePassedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IScoreGatesControllers _scoreGatesControllers;
        private IMoleHitScoreEffectController _scoreEffectController;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IMatchPlayerUIControllers _playerUIControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _scoreGatesControllers = _diContainer.Resolve<IScoreGatesControllers>();
            _scoreEffectController = _diContainer.Resolve<IMoleHitScoreEffectController>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var scoreGatePassedNetEvents = _cachedPresentationEventsService.ScoreGatePassedNetEvents;

            if (scoreGatePassedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var scoreGatePassedNetEvent in scoreGatePassedNetEvents)
            {
                if (_scoreGatesControllers.TryGetScoreGatePosition(scoreGatePassedNetEvent.ScoreGateId, out var gatePosition))
                {
                    _scoreEffectController.PlayEffect(scoreGatePassedNetEvent.ScoreGained, gatePosition);
                }

                _scoreGatesControllers.SetTeamColor(scoreGatePassedNetEvent.ScoreGateId, scoreGatePassedNetEvent.ByTeamId);
                _scoreGatesControllers.PlayScoreGatePassedAnimation(scoreGatePassedNetEvent.ScoreGateId);
                _scoreGatesControllers.SetScoreMultiplier(scoreGatePassedNetEvent.ScoreGateId, scoreGatePassedNetEvent.NewScoreMultiplier);
                _teamsBoardUIController.UpdateTeamMolesHit(scoreGatePassedNetEvent.ByTeamId, scoreGatePassedNetEvent.TeamBonusScoreTotal);
                _playerUIControllers.UpdatePlayerMolesHitScore(scoreGatePassedNetEvent.ByPlayerId, scoreGatePassedNetEvent.ByPlayerBonusScoreTotal);
            }

            _audioService.PlayAudio(AudioClipType.MoleHit); // reuses the bonus-score sound until a dedicated gate clip exists
            scoreGatePassedNetEvents.Clear();
        }
    }
}
