using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleMoleExpiredNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMoleControllers _moleControllers;
        private NetworkConfig _networkConfig;

        private int _tick;

        public HandleMoleExpiredNetEventsCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            var moleExpiredNetEvents = _cachedPresentationEventsService.MoleExpiredNetEvents;

            if (moleExpiredNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleExpiredNetEvent in moleExpiredNetEvents)
            {
                var remainingShakeSeconds = (moleExpiredNetEvent.HideOnTick - _tick) * _networkConfig.DeltaTime;
                _moleControllers.SetMoleExpiring(moleExpiredNetEvent.MoleId, remainingShakeSeconds);
            }

            moleExpiredNetEvents.Clear();
        }
    }
}
