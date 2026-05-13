using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleKOProjectileCreatedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IKOProjectilesControllers _koProjectilesControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.CreateKOProjectileNetEvents;
            if (netEvents.Count == 0) return;

            foreach (var netEvent in netEvents)
            {
                var koProjectileModel = netEvent.KoProjectile;
                var playerCasterId = koProjectileModel.PlayerCasterId;
                var playerCasterPosition = _playerControllers.GetPlayerPosition(playerCasterId);

                _koProjectilesControllers.CreateKOProjectile(koProjectileModel.Id, koProjectileModel.Position.ToUnityVector2(), koProjectileModel.Rotation.ToUnityVector2(),
                    playerCasterPosition, koProjectileModel.Size);
            }
            
            _cachedPresentationEventsService.CreateKOProjectileNetEvents.Clear();
        }
    }
}
