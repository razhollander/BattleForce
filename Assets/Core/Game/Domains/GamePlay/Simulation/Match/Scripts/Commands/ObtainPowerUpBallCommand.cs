using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class ObtainPowerUpBallCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private INetEventsDataService _netEventsDataService;
        private IPlayersPowerUpsManager _playersPowerUpsManager;

        private int _processedTick;
        private ushort _powerUpBallId;
        private ushort _obtainedByPlayerId;

        public ObtainPowerUpBallCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public ObtainPowerUpBallCommand SetPowerUpBallId(ushort powerUpBallId)
        {
            _powerUpBallId = powerUpBallId;
            return this;
        }

        public ObtainPowerUpBallCommand SetObtainedByPlayerId(ushort obtainedByPlayerId)
        {
            _obtainedByPlayerId = obtainedByPlayerId;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _playersPowerUpsManager = _diContainer.Resolve<IPlayersPowerUpsManager>();
        }

        public void Execute()
        {
            if (!_matchDataService.SimulationState.TryGetPowerUpBallIndexById(_powerUpBallId, out _))
            {
                return;
            }

            var powerUpBallBody = _physicsSimulator.GetPowerUpBall(_powerUpBallId);
            _matchDataService.SimulationState.RemovePowerUpBallById(_powerUpBallId);
            _physicsSimulator.RemoveBody(powerUpBallBody);

            _netEventsDataService.AddPowerUpObtainedNetEvent(_processedTick, _powerUpBallId, _obtainedByPlayerId);
            _playersPowerUpsManager.TryGrantRandomPowerUp(_obtainedByPlayerId, _processedTick);
        }
    }
}
