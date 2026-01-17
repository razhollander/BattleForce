using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class HandleBulletDestroyedNetEventsCommand: BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IBulletControllers _bulletControllers;

        public override void ResolveDependencies()
        {
            _bulletControllers = _diContainer.Resolve<IBulletControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var bulletDestroyedNetEvents = _cachedPresentationEventsService.BulletDestroyedNetEvents;
            if (bulletDestroyedNetEvents.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var bulletDestroyedNetEvent in bulletDestroyedNetEvents)
            {
                _bulletControllers.DestroyBullet(bulletDestroyedNetEvent.BulletId);
            }
            
            bulletDestroyedNetEvents.Clear();
        }
    }
}