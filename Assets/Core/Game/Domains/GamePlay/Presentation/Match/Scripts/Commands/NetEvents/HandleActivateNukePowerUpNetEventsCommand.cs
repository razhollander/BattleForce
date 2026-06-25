using Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateNukePowerUpNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private INukeShockwaveEffectController _nukeShockwaveEffectController;
        private IWorldCameraController _worldCameraController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _nukeShockwaveEffectController = _diContainer.Resolve<INukeShockwaveEffectController>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.ActivateNukePowerUpNetEvents.Count == 0)
                return;

            foreach (var netEvent in _cachedPresentationEventsService.ActivateNukePowerUpNetEvents)
            {
                var position = netEvent.CasterPosition.ToUnityVector2();
                _nukeShockwaveEffectController.PlayEffect(position);
                _worldCameraController.ShakeCamera(15, 0.6f);
            }

            _cachedPresentationEventsService.ActivateNukePowerUpNetEvents.Clear();
        }
    }
}
