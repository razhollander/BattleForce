using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryPerformShootForPlayerIfNotOnCooldownCommand : BaseCommand, ICommandVoid
    {
        private IPhysicsSimulator _physicsSimulator;
        private NetworkConfig _networkConfig;
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private SimulationGamePlayConfig _gamePlayConfig;
        private ushort _playerId;
        private int _processedTick;

        public TryPerformShootForPlayerIfNotOnCooldownCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public TryPerformShootForPlayerIfNotOnCooldownCommand SetTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
        }

        public void Execute()
        {
            var playerModel = _matchDataService.SimulationState.GetPlayerById(_playerId);
            var shootState = playerModel.Spaceship.Shoot;
            var shouldShoot = shootState.CooldownSecondsLeft == shootState.MaxCooldown;
            if (!shouldShoot)
            {
                return;
            }

            shootState.CooldownSecondsLeft -= _networkConfig.DeltaTime;
            playerModel.Spaceship.Shoot = shootState;
            CreateBulletForPlayer(_processedTick, playerModel);
        }
        
        private void CreateBulletForPlayer(int processedTick, PlayerStateS2C playerModel)
        {
            var bullet = _matchDataService.AddBullet(playerModel.Id, playerModel.Spaceship.Transform.GetHeadPosition(),
                playerModel.Spaceship.Transform.Direction, _gamePlayConfig.PlayerBullet.MoveSpeed, _gamePlayConfig.PlayerBullet.Radius);
            _netEventsDataService.AddBulletSpawnNetEvent(processedTick, bullet.Id, bullet.BelongToPlayerId, bullet.Position, bullet.Radius);
            _physicsSimulator.AddPlayerBullet(bullet.Id, playerModel.TeamId, bullet.Position, bullet.Velocity, bullet.Radius);
            LogService.LogTopic($"CreateBulletForPlayer {bullet.ToJson()}", LogTopicType.ServerNetwork);
        }
    }
}