using System;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Extensions;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService
{
    public class PlayersDecelerationLogic : IPlayersDecelerationLogic
    {
        private readonly SimulationGamePlayConfig _simulationGamePlayConfig;

        public PlayersDecelerationLogic(SimulationGamePlayConfig simulationGamePlayConfig)
        {
            _simulationGamePlayConfig = simulationGamePlayConfig;
        }

        public void DeceleratePlayerSpin(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTime)
        {
            // 1. Calculate the Damping Factor
            // SpinDampingPerSecond should be a value between 0 and 1 (e.g., 0.5 to lose 50% speed per second).
            float dampingFactor = MathF.Pow(_simulationGamePlayConfig.PlayerSpaceship.SpinDecelerationPerSecond, deltaTime);
    
            // 2. Apply Damping (Works for both clockwise and counter-clockwise)
            playerSpaceshipState.Transform.AngularVelocity *= dampingFactor;

            // 3. Stop if the spin is negligible
            if (MathF.Abs(playerSpaceshipState.Transform.AngularVelocity) < _simulationGamePlayConfig.PlayerSpaceship.MinSpin) 
            {
                playerSpaceshipState.Transform.AngularVelocity = 0;
            }

            // 4. Apply Rotation in Degrees
            if (playerSpaceshipState.Transform.AngularVelocity != 0)
            {
                // Velocity (deg/s) * Time (s) = Rotation for this frame (deg)
                var rotationDegrees = playerSpaceshipState.Transform.AngularVelocity * deltaTime;
                playerSpaceshipState.Transform.Direction.Rotate(rotationDegrees);
            }
            
            //playerSpaceshipState.Transform.AngularVelocity -= deltaTIme * _simulationGamePlayConfig.PlayerSpaceship.SpinDampingPerSecond;
            //
            //     if (playerSpaceshipState.Transform.AngularVelocity < 0)
            //     {
            //         playerSpaceshipState.Transform.AngularVelocity = 0;
            //     }
            //     else
            //     {
            //         playerSpaceshipState.Transform.Direction.Rotate(playerSpaceshipState.Transform.AngularVelocity);
            //     }
        }
        
        public void DeceleratePlayerVelocity(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTIme)
        {
            var playerMovementSpeed = playerSpaceshipState.Transform.Velocity.Length();
            var newSpeed = playerMovementSpeed - _simulationGamePlayConfig.PlayerSpaceship.VelocityDecelerationPerSecond * deltaTIme;
            playerSpaceshipState.Transform.Velocity = playerSpaceshipState.Transform.Velocity.Normalize() * newSpeed;

            // if (playerSpaceshipState.IsEngineOn) // add a force applied to the player every frame, and remove it when reaching the target speed
            // {
            //     var isPlayerAtTargetMovementSpeed = Mathf.Approximately(playerMovementSpeed, targetMovementSpeed);
            //     if (isPlayerAtTargetMovementSpeed)
            //     {
            //         newSpeed = targetMovementSpeed;
            //     }
            //     else
            //     {
            //         var isAboveTargetMovementSpeed = playerMovementSpeed > targetMovementSpeed;
            //         if (isAboveTargetMovementSpeed)
            //         {
            //             newSpeed = playerMovementSpeed - _simulationGamePlayConfig.PlayerSpaceship.VelocityDampingPerSecond * deltaTIme;
            //         }
            //         else
            //         {
            //             var engineForce = _simulationGamePlayConfig.PlayerSpaceship.EngineAcceleration * deltaTIme * playerSpaceshipState.Transform.Direction;
            //             AddForceToPlayer(playerSpaceshipState, engineForce);
            //             newSpeed = playerSpaceshipState.Transform.Velocity.Length();
            //             var didPassTargetMovementSpeed = newSpeed > targetMovementSpeed;
            //
            //             if (didPassTargetMovementSpeed)
            //             {
            //                 newSpeed = targetMovementSpeed;
            //             }
            //         }
            //     }
            // }
            // else
            // {
            //     var isIdle = playerMovementSpeed <= _simulationGamePlayConfig.PlayerSpaceship.IdleMovementSpeed;
            //     if (isIdle)
            //     {
            //         newSpeed = playerMovementSpeed;
            //     }
            //     else
            //     {
            //         newSpeed = playerMovementSpeed - _simulationGamePlayConfig.PlayerSpaceship.VelocityDampingPerSecond * deltaTIme;
            //     }
            // }
            
            //playerSpaceshipState.Transform.Velocity = playerSpaceshipState.Transform.Velocity.Normalize() * newSpeed;
        }
    }
}