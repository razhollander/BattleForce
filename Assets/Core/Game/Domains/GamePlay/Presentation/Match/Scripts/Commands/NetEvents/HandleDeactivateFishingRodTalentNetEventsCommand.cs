using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateFishingRodTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IFishingRodTipControllers _fishingRodTipControllers;
        private ISecondCastAimArrowControllers _secondCastAimArrowControllers;
        private IMatchDataService _matchDataService;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _secondCastAimArrowControllers = _diContainer.Resolve<ISecondCastAimArrowControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DeactivateFishingRodTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                // Deactivating is the only way a tip stops holding a caught enemy, so it is also the only place the
                // throw-aim arrow needs removing. Tips that never caught an enemy have no arrow.
                _secondCastAimArrowControllers.TryRemoveArrow(netEvent.ProjectileId);
                _fishingRodTipControllers.DestroyFishingRodTip(netEvent.ProjectileId);
                _matchDataService.RemoveFishingRodTip(netEvent.ProjectileId);
                _playerControllers.SetPlayerFishingRodStickState(netEvent.CasterPlayerId, false);
            }

            _cachedPresentationEventsService.DeactivateFishingRodTalentNetEvents.Clear();
        }
    }
}
