using Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGainedEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleMoleHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private const ushort GOLDEN_MOLE_DAMAGE_PER_HIT = 1; // a golden mole loses one life per hit, shown on its damage indicator

        private ICachedPresentationEventsService _cachedPresentationEventsService;
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
            var moleHitNetEvents = _cachedPresentationEventsService.MoleHitNetEvents;

            if (moleHitNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleHitNetEvent in moleHitNetEvents)
            {
                if (moleHitNetEvent.IsGolden && _moleControllers.TryGetMoleHolePosition(moleHitNetEvent.MoleHoleId, out var molePosition))
                {
                    _hitDamageIndicatorEffectController.PlayEffect(GOLDEN_MOLE_DAMAGE_PER_HIT, molePosition);
                }
                
                var playerPosition = _playerControllers.GetPlayerPosition(moleHitNetEvent.ByPlayerId);
                _scoreGainedEffectController.PlayEffect(moleHitNetEvent.ScoreGained, playerPosition);

                _moleControllers.SetMoleHit(moleHitNetEvent.MoleId, moleHitNetEvent.MoleHoleId);
                _teamsBoardUIController.UpdateTeamMolesHit(moleHitNetEvent.ByTeamId, moleHitNetEvent.TeamMolesHitTotal);
                _playerUIControllers.UpdatePlayerMolesHitScore(moleHitNetEvent.ByPlayerId, moleHitNetEvent.ByPlayerMolesHitScoreTotal);
            }

            _audioService.PlayAudio(AudioClipType.MoleHit);
            moleHitNetEvents.Clear();
        }
    }
}
