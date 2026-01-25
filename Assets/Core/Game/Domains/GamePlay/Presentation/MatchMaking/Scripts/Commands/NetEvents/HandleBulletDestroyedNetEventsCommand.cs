using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandleBulletDestroyedNetEventsCommand: BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchMakingBulletControllers _bulletControllers;

        public override void ResolveDependencies()
        {
            _bulletControllers = _diContainer.Resolve<IMatchMakingBulletControllers>();
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