using Core.Game.Domains.GamePlay.Presentation.Match.Features.MagneticPullEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleCreateMagenticPullFieldNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMagneticPullEffectController _magneticPullEffectController;
        private IMatchPlayerControllers _matchPlayerControllers;
        private SharedGamePlayConfig _sharedConfig;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _magneticPullEffectController = _diContainer.Resolve<IMagneticPullEffectController>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _sharedConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.CreateMagenticPullFieldNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.CreateMagenticPullFieldNetEvents)
            {
                _magneticPullEffectController.PlayFieldEffect(netEvent.Position.ToUnityVector2(), netEvent.Direction.ToUnityVector2(), _sharedConfig.MagneticPullFieldRadius);
                _audioService.PlayAudio(AudioClipType.MagneticPullCast);

                if (netEvent.HasHit)
                {
                    var enemyPosition = _matchPlayerControllers.GetPlayerPosition(netEvent.HitEnemyId);
                    var casterPosition = _matchPlayerControllers.GetPlayerPosition(netEvent.CasterPlayerId);
                    _magneticPullEffectController.PlayHitEffect(casterPosition, enemyPosition);
                }
            }

            _cachedPresentationEventsService.CreateMagenticPullFieldNetEvents.Clear();
        }
    }
}