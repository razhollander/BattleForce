using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleEnvironmentSpringPlayerCollisionNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IMatchEnvironmentSpringControllers _environmentSpringControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _environmentSpringControllers = _diContainer.Resolve<IMatchEnvironmentSpringControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
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
                var spring = _environmentSpringControllers.GetSpring(collisionEvent.SpringId);
                spring?.PlayBounceAnimation();
            }

            environmentSpringPlayerCollisionNetEvents.Clear();
        }
    }
}
