using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class HandleBulletDestroyedNetEventsCommand: BaseCommand, ICommandVoid
    {
        private IMatchNetEventsDataService _matchNetEventsDataService;
        private IBulletControllers _bulletControllers;

        public override void ResolveDependencies()
        {
            _bulletControllers = _diContainer.Resolve<IBulletControllers>();
            _matchNetEventsDataService = _diContainer.Resolve<IMatchNetEventsDataService>();
        }

        public void Execute()
        {
            var bulletDestroyedNetEvents = _matchNetEventsDataService.BulletDestroyedNetEvents;
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