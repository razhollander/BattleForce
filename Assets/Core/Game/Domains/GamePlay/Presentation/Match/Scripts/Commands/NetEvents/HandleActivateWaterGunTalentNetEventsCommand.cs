using Core.Game.Domains.GamePlay.Presentation.Match.Features.WaterGunStream.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateWaterGunTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IWaterGunStreamControllers _waterGunStreamControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _waterGunStreamControllers = _diContainer.Resolve<IWaterGunStreamControllers>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.ActivateWaterGunTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var evt in events)
            {
                _waterGunStreamControllers.Spawn(evt.CasterPlayerId);
            }

            _cachedPresentationEventsService.ActivateWaterGunTalentNetEvents.Clear();
        }
    }
}
