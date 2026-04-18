using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateSentryGunTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DeactivateSentryGunTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                _matchPlayerControllers.SetPlayerSentryGunState(netEvent.CasterPlayerId, false);
                var casterPlayerId = netEvent.CasterPlayerId;

                if (_matchDataService.GetPlayer(casterPlayerId).Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentSelectedTalentForCaster))
                {
                    _matchPlayerControllers.UpdateIsPlayerArrowShownAccordingToTalentState(casterPlayerId, currentSelectedTalentForCaster);
                }
            }

            _cachedPresentationEventsService.DeactivateSentryGunTalentNetEvents.Clear();
        }
    }
}
