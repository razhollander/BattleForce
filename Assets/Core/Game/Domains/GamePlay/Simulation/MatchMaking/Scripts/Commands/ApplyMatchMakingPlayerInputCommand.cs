using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.PlayerLockOnWall;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands
{
    public class ApplyMatchMakingPlayerInputCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private NetworkConfig _networkConfig;
        private TryShootLockedOnWallCommand _tryShootLockedOnWallCommand;

        private MatchMakingLocalPlayerInputDataC2S _playerInputData;
        private int _processedTick;

        public ApplyMatchMakingPlayerInputCommand SetPlayerInputData(MatchMakingLocalPlayerInputDataC2S playerInputData)
        {
            _playerInputData = playerInputData;
            return this;
        }

        public ApplyMatchMakingPlayerInputCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            var commandFactory = _diContainer.Resolve<ICommandFactory>();
            _tryShootLockedOnWallCommand = commandFactory.CreateCommandVoid<TryShootLockedOnWallCommand>();
        }

        public void Execute()
        {
            var playerId = _playerInputData.PlayerId;
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);
            if (playerState == null)
            {
                LogService.LogTopic($"Didn't find player state for player {playerId}!", LogTopicType.ServerNetwork);
                return;
            }

            UpdatePlayerDirection(playerState);
            UpdatePlayerShoot(playerState);
        }

        private void UpdatePlayerDirection(MatchMakingPlayerStateS2C playerState)
        {
            var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (_playerInputData.IsMoveLeftInputPressed.ToInt() -
                 _playerInputData.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            playerState.Spaceship.Transform.Direction = playerState.Spaceship.Transform.Direction.Rotate(rotationAngle);

            if (_playerInputData.IsMoveForwardInputPressed)
            {
                playerState.Spaceship.Transform.Velocity = playerState.Spaceship.Transform.Direction * _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.TargetMovementSpeed;
            }
            else
            {
                playerState.Spaceship.Transform.Velocity = System.Numerics.Vector2.Zero;
            }
        }

        private void UpdatePlayerShoot(MatchMakingPlayerStateS2C playerState)
        {
            var isShootInputPressed = _playerInputData.IsShootInputPressed;
            if (!_gamePlayConfigService.GamePlayConfig.IsAutoShoot && !isShootInputPressed)
            {
                return;
            }

            _tryShootLockedOnWallCommand.SetCasterPlayerId(playerState.Id).SetTick(_processedTick).Execute();
        }
    }
}
