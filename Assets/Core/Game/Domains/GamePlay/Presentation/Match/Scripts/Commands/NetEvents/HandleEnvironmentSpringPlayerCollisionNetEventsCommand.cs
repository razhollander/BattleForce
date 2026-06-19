using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleEnvironmentSpringPlayerCollisionNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IEnvironmentSpringControllers _environmentSpringControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _environmentSpringControllers = _diContainer.Resolve<IEnvironmentSpringControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var environmentSpringPlayerCollisionNetEvents = _cachedPresentationEventsService.EnvironmentSpringPlayerCollisionNetEvents;
            if (environmentSpringPlayerCollisionNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var collisionEvent in environmentSpringPlayerCollisionNetEvents)
            {
                _environmentSpringControllers.PlaySpringBounceAnimation(collisionEvent.SpringId);
                _audioService.PlayAudio(AudioClipType.EnvironmentSpringCollision);
            }

            environmentSpringPlayerCollisionNetEvents.Clear();
        }
    }
}
