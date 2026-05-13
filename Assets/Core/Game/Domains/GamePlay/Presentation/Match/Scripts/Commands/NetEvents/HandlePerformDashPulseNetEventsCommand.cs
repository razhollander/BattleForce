using Core.Game.Domains.GamePlay.Presentation.Match.Features.DashPulse.Scripts.Effect;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandlePerformDashPulseNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IDashPulseGustEffectController _dashPulseGustEffectController;
        private IMatchDataService _matchDataService;
        private IMatchPlayerControllers _playerControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _dashPulseGustEffectController = _diContainer.Resolve<IDashPulseGustEffectController>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PerformDashPulseNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.PerformDashPulseNetEvents)
            {
                PlayDashPulseEffectForPlayer(netEvent.CasterPlayerId);
            }

            _cachedPresentationEventsService.PerformDashPulseNetEvents.Clear();
        }

        private void PlayDashPulseEffectForPlayer(ushort playerId)
        {
            var casterPlayer = _matchDataService.GetPlayer(playerId);
            var position = casterPlayer.Spaceship.Transform.Position.ToUnityVector2();
            var direction = casterPlayer.Spaceship.Transform.Direction.ToUnityVector2();

            _dashPulseGustEffectController.PlayEffect(position, direction);
        }
    }
}
