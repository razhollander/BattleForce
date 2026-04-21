using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleLayChickenEggNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchChickenEggsControllers _chickenEggsControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.LayChickenEggNetEvents;
            if (netEvents.IsNullOrEmpty()) return;

            foreach (var netEvent in netEvents)
            {
                _matchPlayerControllers.PlayLayEggAnimation(netEvent.CasterPlayerId);
                _chickenEggsControllers.CreateEgg(netEvent.EggId, netEvent.Position);
            }

            netEvents.Clear();
        }
    }
}
