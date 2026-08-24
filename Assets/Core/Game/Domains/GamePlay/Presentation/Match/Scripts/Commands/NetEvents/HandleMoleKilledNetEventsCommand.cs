using Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGainedEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleMoleKilledNetEventsCommand : BaseCommand, ICommandVoid
    {
        private const ushort GOLDEN_MOLE_DAMAGE_PER_HIT = 1; // a golden mole loses one life per hit, shown on its damage indicator

        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchDataService _matchDataService;
        private IMoleControllers _moleControllers;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IAudioService _audioService;
        private IScoreGainedEffectController _scoreGainedEffectController;
        private IHitDamageIndicatorEffectController _hitDamageIndicatorEffectController;
        private IMatchPlayerControllers _playerControllers;
        private IMatchPlayerUIControllers _playerUIControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _scoreGainedEffectController = _diContainer.Resolve<IScoreGainedEffectController>();
            _hitDamageIndicatorEffectController = _diContainer.Resolve<IHitDamageIndicatorEffectController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
        }

        public void Execute()
        {
            var moleKilledNetEvents = _cachedPresentationEventsService.MoleKilledNetEvents;

            if (moleKilledNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleKilledNetEvent in moleKilledNetEvents)
            {
                if (moleKilledNetEvent.IsGolden && _moleControllers.TryGetMoleHolePosition(moleKilledNetEvent.MoleHoleId, out var molePosition))
                {
                    _hitDamageIndicatorEffectController.PlayEffect(GOLDEN_MOLE_DAMAGE_PER_HIT, molePosition);
                }
                
                var playerPosition = _playerControllers.GetPlayerPosition(moleKilledNetEvent.ByPlayerId);
                _scoreGainedEffectController.PlayEffect(moleKilledNetEvent.ScoreGained, playerPosition);

                _moleControllers.SetMoleKilled(moleKilledNetEvent.MoleId, moleKilledNetEvent.MoleHoleId);
                var byTeamId = _matchDataService.GetPlayerTeamId(moleKilledNetEvent.ByPlayerId);
                _teamsBoardUIController.UpdateTeamMolesKilled(byTeamId, moleKilledNetEvent.TeamMolesKilledTotal);
                _playerUIControllers.UpdatePlayerMolesKilledScore(moleKilledNetEvent.ByPlayerId, moleKilledNetEvent.ByPlayerMolesKilledScoreTotal);
            }

            _audioService.PlayAudio(AudioClipType.MoleKilled);
            moleKilledNetEvents.Clear();
        }
    }
}
