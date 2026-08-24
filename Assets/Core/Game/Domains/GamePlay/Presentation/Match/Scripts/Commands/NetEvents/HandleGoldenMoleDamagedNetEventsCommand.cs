using Core.Game.Domains.GamePlay.Presentation.Match.Features.HitDamageIndicatorEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleGoldenMoleDamagedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMoleControllers _moleControllers;
        private IHitDamageIndicatorEffectController _hitDamageIndicatorEffectController;
        private IAudioService _audioService;
        private SharedGamePlayConfig _sharedGamePlayConfig;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _hitDamageIndicatorEffectController = _diContainer.Resolve<IHitDamageIndicatorEffectController>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        {
            var goldenMoleDamagedNetEvents = _cachedPresentationEventsService.GoldenMoleDamagedNetEvents;

            if (goldenMoleDamagedNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var goldenMoleDamagedNetEvent in goldenMoleDamagedNetEvents)
            {
                if (_moleControllers.TryGetMoleHolePosition(goldenMoleDamagedNetEvent.MoleHoleId, out var molePosition))
                {
                    _hitDamageIndicatorEffectController.PlayEffect(_sharedGamePlayConfig.GoldenMoleDamagePerHit, molePosition);
                }

                _moleControllers.SetGoldenMoleDamaged(goldenMoleDamagedNetEvent.MoleId, goldenMoleDamagedNetEvent.MoleHoleId,
                    goldenMoleDamagedNetEvent.RemainingLives, goldenMoleDamagedNetEvent.MaxLives);
            }

            _audioService.PlayAudio(AudioClipType.MoleKilled);
            goldenMoleDamagedNetEvents.Clear();
        }
    }
}
