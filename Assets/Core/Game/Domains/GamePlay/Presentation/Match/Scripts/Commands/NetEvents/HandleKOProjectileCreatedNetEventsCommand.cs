using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.KOProjectiles.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleKOProjectileCreatedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IKOProjectilesControllers _koProjectilesControllers;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _koProjectilesControllers = _diContainer.Resolve<IKOProjectilesControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.CreateKOProjectileNetEvents;
            if (netEvents.Count == 0) return;

            foreach (var koProjectileCreatedNetEvent in netEvents)
            {
                var koProjectileModel = koProjectileCreatedNetEvent.KoProjectile;
                var playerCasterPosition = _playerControllers.GetPlayerPosition(koProjectileModel.PlayerCasterId);

                _koProjectilesControllers.CreateKOProjectile(koProjectileModel.Id, koProjectileModel.Position.ToUnityVector2(), koProjectileModel.Rotation.ToUnityVector2(),
                    playerCasterPosition, koProjectileModel.Size);
            }
            
            _cachedPresentationEventsService.CreateKOProjectileNetEvents.Clear();
        }
    }
}
