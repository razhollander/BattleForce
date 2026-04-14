using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using System.Numerics;
using System;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class SpinPlayerCommand : BaseCommand, ICommandVoid
    {
        private INetEventsDataService _netEventsDataService;

        private PlayerSpaceshipStateS2C _spaceshipState;
        private ushort _playerId;
        private float _spinAmount;
        private int _tick;

        public SpinPlayerCommand SetPlayer(ushort playerId, PlayerSpaceshipStateS2C spaceshipState)
        {
            _playerId = playerId;
            _spaceshipState = spaceshipState;
            return this;
        }

        public SpinPlayerCommand SetSpinAmount(float spinAmount)
        {
            _spinAmount = spinAmount;
            return this;
        }

        public SpinPlayerCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            if (_spaceshipState == null)
            {
                return;
            }

            var wasSpinning = _spaceshipState.Transform.AngularVelocity != 0;

            _spaceshipState.Transform.AngularVelocity += _spinAmount;

            var isSpinningNow = _spaceshipState.Transform.AngularVelocity != 0;

            if (!wasSpinning && isSpinningNow)
            {
                _netEventsDataService.AddPlayerSpinnedStartedNetEvent(_tick, _playerId);
            }
        }
    }
}
