using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleCreateFishingRodProjectileNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IFishingRodTipControllers _fishingRodTipControllers;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.CreateFishingRodProjectileNetEvents;
            if (netEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                var tipModel = netEvent.FishingRodProjectile;
                var casterPlayerId = tipModel.PlayerCasterId;

                // The stick must be shown before we read its tip pivot, since the fishing line starts from that pivot.
                _playerControllers.SetPlayerFishingRodStickState(casterPlayerId, true);

                var casterPosition = _playerControllers.GetPlayerPosition(casterPlayerId);
                var lineStartPosition = _playerControllers.GetPlayerFishingRodTipPivotPosition(casterPlayerId);
                var rotation = tipModel.Position - casterPosition.ToNumericsVector2();

                _fishingRodTipControllers.CreateFishingRodTip(tipModel.Id, casterPlayerId, tipModel.Position.ToUnityVector2(), rotation.ToUnityVector2(), lineStartPosition);
            }

            _cachedPresentationEventsService.CreateFishingRodProjectileNetEvents.Clear();
        }
    }
}
