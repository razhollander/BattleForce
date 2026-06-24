using Core.Game.Domains.GamePlay.Presentation.Match.Features.Nuke.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleActivateNukePowerUpNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private INukeShockwaveEffectController _nukeShockwaveEffectController;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _nukeShockwaveEffectController = _diContainer.Resolve<INukeShockwaveEffectController>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.ActivateNukePowerUpNetEvents.Count == 0)
                return;

            foreach (var netEvent in _cachedPresentationEventsService.ActivateNukePowerUpNetEvents)
            {
                var position = netEvent.CasterPosition.ToUnityVector2();
                _nukeShockwaveEffectController.PlayEffect(position);
            }

            _cachedPresentationEventsService.ActivateNukePowerUpNetEvents.Clear();
        }
    }
}
