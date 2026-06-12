using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Spikes.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleEnvironmentSpikePlayerCollisionNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IEnvironmentSpikeControllers _environmentSpikeControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _environmentSpikeControllers = _diContainer.Resolve<IEnvironmentSpikeControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var environmentSpikePlayerCollisionNetEvents = _cachedPresentationEventsService.EnvironmentSpikePlayerCollisionNetEvents;
            if (environmentSpikePlayerCollisionNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var collisionEvent in environmentSpikePlayerCollisionNetEvents)
            {
                _environmentSpikeControllers.PlaySpikeBounceAnimation(collisionEvent.SpikeId);
            }

            environmentSpikePlayerCollisionNetEvents.Clear();
        }
    }
}
