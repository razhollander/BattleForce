using Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateGalacticForceFieldNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IGalacticPullStarEffectControllers _galacticPullStarEffectControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _galacticPullStarEffectControllers = _diContainer.Resolve<IGalacticPullStarEffectControllers>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DeactivateGalacticForceFieldNetEvents;
            if (events.Count == 0)
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _galacticPullStarEffectControllers.HideStarForceField(netEvent.GalacticForceFieldId);
            }

            events.Clear();
        }
    }
}
