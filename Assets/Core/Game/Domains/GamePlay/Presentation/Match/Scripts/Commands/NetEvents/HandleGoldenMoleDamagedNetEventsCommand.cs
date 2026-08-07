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
        private const ushort GOLDEN_MOLE_DAMAGE_PER_HIT = 1; // a golden mole loses one life per hit, shown on its damage indicator

        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMoleControllers _moleControllers;
        private IHitDamageIndicatorEffectController _hitDamageIndicatorEffectController;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _hitDamageIndicatorEffectController = _diContainer.Resolve<IHitDamageIndicatorEffectController>();
            _audioService = _diContainer.Resolve<IAudioService>();
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
                if (_moleControllers.TryGetMolePosition(goldenMoleDamagedNetEvent.MoleId, out var molePosition))
                {
                    _hitDamageIndicatorEffectController.PlayEffect(GOLDEN_MOLE_DAMAGE_PER_HIT, molePosition);
                }

                _moleControllers.SetGoldenMoleDamaged(goldenMoleDamagedNetEvent.MoleId, goldenMoleDamagedNetEvent.RemainingLives,
                    goldenMoleDamagedNetEvent.MaxLives);
            }

            _audioService.PlayAudio(AudioClipType.MoleHit);
            goldenMoleDamagedNetEvents.Clear();
        }
    }
}
