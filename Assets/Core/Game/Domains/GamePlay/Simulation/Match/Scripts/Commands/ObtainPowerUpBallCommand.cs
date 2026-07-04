using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class ObtainPowerUpBallCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private INetEventsDataService _netEventsDataService;
        private IPlayersPowerUpsManager _playersPowerUpsManager;
        private ISimulationGamePlayConfigService _gamePlayConfigService;

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
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
        }

        public void Execute()
        {
            if (!_matchDataService.SimulationState.TryGetPowerUpBallIndexById(_powerUpBallId, out _))
            {
                LogService.Log("PowerUpBall not found in match data service, probably already got shot the tick");
                return;
            }

            var powerUpBallBody = _physicsSimulator.GetPowerUpBall(_powerUpBallId);
            _matchDataService.SimulationState.RemovePowerUpBallById(_powerUpBallId);
            _physicsSimulator.RemoveBody(powerUpBallBody);

            _netEventsDataService.AddPowerUpObtainedNetEvent(_processedTick, _powerUpBallId, _obtainedByPlayerId);
            var grantedPowerUp = GetRandomObtainablePowerUp();
            _playersPowerUpsManager.TryGrantPowerUp(_obtainedByPlayerId, grantedPowerUp, _processedTick);
        }
        
        private PowerUpType GetRandomObtainablePowerUp()
        {
            var obtainablePowerUps = _gamePlayConfigService.GamePlayConfig.PowerUps.ObtainablePowerUps;
            var randomIndex = RNG.NextInt(0, obtainablePowerUps.Length);
            return obtainablePowerUps[randomIndex];
        }
    }
}
