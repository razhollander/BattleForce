using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleLayChickenEggNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchChickenEggsControllers _chickenEggsControllers;
        private Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService.IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _matchDataService = _diContainer.Resolve<Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService.IMatchDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.LayChickenEggNetEvents;
            if (events.IsNullOrEmpty()) return;

            foreach (var evt in events)
            {
                _matchPlayerControllers.PlayLayEggAnimation(evt.CasterPlayerId);
                _matchDataService.AddChickenEgg(evt.EggId, evt.Position.ToUnityVector2(), false);
                _chickenEggsControllers.CreateEgg(evt.EggId, evt.Position);
            }

            events.Clear();
        }
    }
}
