using Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.MoleHitScoreEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
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
        private IMoleHitScoreEffectController _moleHitScoreEffectController;
        private IHitDamageIndicatorEffectController _hitDamageIndicatorEffectController;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _moleHitScoreEffectController = _diContainer.Resolve<IMoleHitScoreEffectController>();
            _hitDamageIndicatorEffectController = _diContainer.Resolve<IHitDamageIndicatorEffectController>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
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
                // The final hit of a golden mole is still a hit, so it shows the damage indicator on the mole as well.
                if (moleHitNetEvent.IsGolden && _moleControllers.TryGetMolePosition(moleHitNetEvent.MoleId, out var molePosition))
                {
                    _hitDamageIndicatorEffectController.PlayEffect(GOLDEN_MOLE_DAMAGE_PER_HIT, molePosition);
                }

                // The score is awarded to the team of the player who landed the hit, so its popup shows on that player.
                var playerPosition = _playerControllers.GetPlayerPosition(moleHitNetEvent.ByPlayerId);
                _moleHitScoreEffectController.PlayEffect(moleHitNetEvent.ScoreGained, playerPosition);

                _moleControllers.SetMoleHit(moleHitNetEvent.MoleId);
                _teamsBoardUIController.UpdateTeamMolesHit(moleHitNetEvent.ByTeamId, moleHitNetEvent.TeamMolesHitTotal);
            }

            _audioService.PlayAudio(AudioClipType.MoleHit);
            moleHitNetEvents.Clear();
        }
    }
}
