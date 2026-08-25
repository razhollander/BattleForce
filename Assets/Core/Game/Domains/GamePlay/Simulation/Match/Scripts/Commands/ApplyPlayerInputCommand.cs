using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    // One tick of one player's input applied to the simulation, in the order the simulation needs it: every input level
    // is recorded before anything reads an edge off it, and the talents run last so they see the direction and the
    // levels this tick already set.
    public class ApplyPlayerInputCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private ISimulationInputService _simulationInputService;
        private IPlayersMouseDataService _playersMouseDataService;
        private IPlayersPowerUpsManager _playersPowerUpsManager;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private NetworkConfig _networkConfig;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private TryShootLockedOnTargetsCommand _tryShootLockedOnTargetsCommand;
        private TryPerformBarrelDashCommand _tryPerformBarrelDashCommand;
        private TrySetPlayerMoveDestinationPointCommand _trySetPlayerMoveDestinationPointCommand;
        private RotatePlayerTowardsMoveDestinationPointCommand _rotatePlayerTowardsMoveDestinationPointCommand;
        private ApplyPlayerTalentsInputCommand _applyPlayerTalentsInputCommand;

        private MatchLocalPlayerInputDataC2S _playerInputData;
        private long _clientId;
        private int _processedTick;
        private float _deltaTime;

        public ApplyPlayerInputCommand SetPlayerInputData(MatchLocalPlayerInputDataC2S playerInputData)
        {
            _playerInputData = playerInputData;
            return this;
        }

        public ApplyPlayerInputCommand SetClientId(long clientId)
        {
            _clientId = clientId;
            return this;
        }

        public ApplyPlayerInputCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public ApplyPlayerInputCommand SetDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _simulationInputService = _diContainer.Resolve<ISimulationInputService>();
            _playersMouseDataService = _diContainer.Resolve<IPlayersMouseDataService>();
            _playersPowerUpsManager = _diContainer.Resolve<IPlayersPowerUpsManager>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            var commandFactory = _diContainer.Resolve<ICommandFactory>();
            _tryShootLockedOnTargetsCommand = commandFactory.CreateCommandVoid<TryShootLockedOnTargetsCommand>();
            _tryPerformBarrelDashCommand = commandFactory.CreateCommandVoid<TryPerformBarrelDashCommand>();
            _trySetPlayerMoveDestinationPointCommand = commandFactory.CreateCommandVoid<TrySetPlayerMoveDestinationPointCommand>();
            _rotatePlayerTowardsMoveDestinationPointCommand = commandFactory.CreateCommandVoid<RotatePlayerTowardsMoveDestinationPointCommand>();
            _applyPlayerTalentsInputCommand = commandFactory.CreateCommandVoid<ApplyPlayerTalentsInputCommand>();
        }

        public void Execute()
        {
            var playerId = _playerInputData.PlayerId;
            var playerState = _matchDataService.SimulationState.GetPlayerById(playerId);

            UpdatePlayerShoot(playerState);
            playerState.Spaceship.AimDirection = _playerInputData.AimDirection;
            _playersMouseDataService.SetPlayerMouseData(playerId, _playerInputData.IsUsingMouseAim, _playerInputData.MouseWorldPosition);
            UpdatePlayerDirection(playerState);

            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentAInput, _playerInputData.IsTalentAInputPressed);
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentBInput, _playerInputData.IsTalentBInputPressed);
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.TalentCInput, _playerInputData.IsTalentCInputPressed);
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.PowerUpInput, _playerInputData.IsPowerUpInputPressed);
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.BarrelDashInput, _playerInputData.IsBarrelDashInputPressed);

            _applyPlayerTalentsInputCommand
                .SetPlayerId(playerId)
                .SetProcessedTick(_processedTick)
                .SetDeltaTime(_deltaTime)
                .SetTalentInputsPressed(_playerInputData.IsTalentAInputPressed, _playerInputData.IsTalentBInputPressed, _playerInputData.IsTalentCInputPressed)
                .Execute();

            ProcessPlayerPowerUpInput(playerId);
            ProcessPlayerBarrelDashInput(playerId);
        }

        private void UpdatePlayerShoot(PlayerStateS2C playerState)
        {
            var playerId = playerState.Id;
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.Shoot, _playerInputData.IsShootInputPressed);

            var wasShootInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.Shoot);
            if (!_gamePlayConfigService.GamePlayConfig.IsAutoShoot && !wasShootInputDownThisTick)
            {
                return;
            }

            _tryShootLockedOnTargetsCommand.SetCasterPlayerId(playerId).SetProcessedTick(_processedTick).Execute();
        }

        private void UpdatePlayerDirection(PlayerStateS2C playerState)
        {
            if (IsPlayerSteeringWithMouse())
            {
                UpdatePlayerDirectionFromMoveDestinationPoint(playerState);
                return;
            }

            if (playerState.Spaceship.TalentsState.IsSelectedTalentBlockingRotation())
            {
                return;
            }

            var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
            var rotationAngle =
                (_playerInputData.IsMoveLeftInputPressed.ToInt() -
                 _playerInputData.IsMoveRightInputPressed.ToInt()) * rotationDelta;
            var rotatedVector = playerState.Spaceship.Transform.Direction.Rotate(rotationAngle);
            playerState.Spaceship.Transform.Direction = rotatedVector;
        }

        private bool IsPlayerSteeringWithMouse()
        {
            var isPlayerOnKeyboard = _playerInputData.IsUsingMouseAim;

            return _sharedGamePlayConfig.ShouldMoveWithMouse && isPlayerOnKeyboard;
        }

        private void UpdatePlayerDirectionFromMoveDestinationPoint(PlayerStateS2C playerState)
        {
            var playerId = playerState.Id;
            _simulationInputService.SetPlayerInput(playerId, PlayerInputType.MoveToPointInput, _playerInputData.IsMoveToPointInputPressed);

            var isRetargetingDestinationPoint = _simulationInputService.IsInputPressed(playerId, PlayerInputType.MoveToPointInput);
            if (isRetargetingDestinationPoint)
            {
                var wasDestinationPointClickedThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.MoveToPointInput);
                _trySetPlayerMoveDestinationPointCommand
                    .SetPlayerId(playerId)
                    .SetDestinationPoint(_playerInputData.MouseWorldPosition)
                    .SetClientId(_clientId)
                    .SetProcessedTick(_processedTick)
                    .ShouldShowIndicator(wasDestinationPointClickedThisTick)
                    .Execute();
            }

            _rotatePlayerTowardsMoveDestinationPointCommand.SetPlayerId(playerId).Execute();
        }

        private void ProcessPlayerPowerUpInput(ushort playerId)
        {
            var wasPowerUpInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.PowerUpInput);
            _playersPowerUpsManager.ProcessPowerUpInput(playerId, _processedTick, wasPowerUpInputDownThisTick);
        }

        private void ProcessPlayerBarrelDashInput(ushort playerId)
        {
            var wasBarrelDashInputDownThisTick = _simulationInputService.WasInputDownThisTick(playerId, PlayerInputType.BarrelDashInput);
            if (!wasBarrelDashInputDownThisTick)
            {
                return;
            }

            _tryPerformBarrelDashCommand.SetPlayerId(playerId).SetProcessedTick(_processedTick).Execute();
        }
    }
}
