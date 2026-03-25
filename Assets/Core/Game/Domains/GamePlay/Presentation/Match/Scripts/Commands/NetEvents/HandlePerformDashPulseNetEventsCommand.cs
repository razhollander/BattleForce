using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePerformDashPulseNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IDashPulseGustEffectController _dashPulseGustEffectController;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _dashPulseGustEffectController = _diContainer.Resolve<IDashPulseGustEffectController>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PerformDashPulseNetEvents.Count == 0)
            {
                return;
            }

            foreach (var evt in _cachedPresentationEventsService.PerformDashPulseNetEvents)
            {
                var casterPlayer = _matchDataService.GetPlayer(evt.CasterPlayerId);
                var position = casterPlayer.Spaceship.Transform.Position.ToUnityVector2();
                var direction = casterPlayer.Spaceship.Transform.Direction.ToUnityVector2();

                _dashPulseGustEffectController.PlayEffect(position, direction);
            }

            _cachedPresentationEventsService.PerformDashPulseNetEvents.Clear();
        }
    }
}
