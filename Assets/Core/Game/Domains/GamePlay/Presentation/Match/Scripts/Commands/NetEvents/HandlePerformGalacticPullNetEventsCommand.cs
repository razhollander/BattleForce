using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePerformGalacticPullNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.PerformGalacticPullNetEvents;
            if (events.Count == 0)
            {
                return;
            }

            foreach (var netEvent in events)
            {
                // TODO: spawn GalacticForceField star visual (Unity side)
                //GalacticForceFieldControllers.CreateField(netEvent.FieldId, netEvent.CasterTeamId, netEvent.EndTick)
            }

            events.Clear();
        }
    }
}
