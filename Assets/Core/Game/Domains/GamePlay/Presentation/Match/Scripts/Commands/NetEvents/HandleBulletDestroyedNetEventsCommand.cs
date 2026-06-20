using Core.Game.Domains.GamePlay.Presentation.Match.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleBulletDestroyedNetEventsCommand: BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchBulletControllers _bulletControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _bulletControllers = _diContainer.Resolve<IMatchBulletControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
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