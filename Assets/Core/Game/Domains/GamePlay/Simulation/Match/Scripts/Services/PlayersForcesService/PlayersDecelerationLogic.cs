using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService
{
    public class PlayersDecelerationLogic : IPlayersDecelerationLogic
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        public PlayersDecelerationLogic(ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _gamePlayConfigService = gamePlayConfigService;
        }

        public void DeceleratePlayerSpin(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTime)
        {
            var dampingFactor = MathF.Pow(_gamePlayConfigService.GamePlayConfig.PlayerSpaceship.SpinDecelerationPerSecond, deltaTime);
            playerSpaceshipState.Transform.AngularVelocity *= dampingFactor;
            
            var isAngularVelocityAlmostZero = MathF.Abs(playerSpaceshipState.Transform.AngularVelocity) < _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.MinSpin;

            if (isAngularVelocityAlmostZero)
            {
                playerSpaceshipState.Transform.AngularVelocity = 0;
            }

            if (playerSpaceshipState.Transform.AngularVelocity != 0)
            {
                var rotationDegrees = playerSpaceshipState.Transform.AngularVelocity;
                playerSpaceshipState.Transform.Direction = playerSpaceshipState.Transform.Direction.Rotate(rotationDegrees);
            }
        }
        
        public void DeceleratePlayerVelocity(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTIme)
        {
            var playerMovementSpeed = playerSpaceshipState.Transform.Velocity.Length();
            var newSpeed = playerMovementSpeed - _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.VelocityDecelerationPerSecond * deltaTIme;
            if (newSpeed < _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.MinVelocity)
            {
                newSpeed = 0;
            }
            
            var normalized = playerSpaceshipState.Transform.Velocity.NormalizeSafe();
            playerSpaceshipState.Transform.Velocity = normalized * newSpeed;
        }
    }
}