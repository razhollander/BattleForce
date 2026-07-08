using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService
{
    public class PlayersEngineLogic : IPlayersEngineLogic
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        public PlayersEngineLogic(ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _gamePlayConfigService = gamePlayConfigService;
        }

        public void TurnOnEngineForPlayerIfPossible(PlayerSpaceshipStateS2C playerSpaceshipState)
        {
            if (playerSpaceshipState.IsEngineOn || !playerSpaceshipState.IsAlive)
            {
                return;
            }

            var isPlayerInSentryGun = playerSpaceshipState.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) 
                                               && selectedTalent is {TalentType: TalentType.SentryGun, IsCurrentlyActive: true};

            if (isPlayerInSentryGun)
            {
                return;
            }

            var isPlayerInUmbrella = selectedTalent is {TalentType: TalentType.Umbrella, IsCurrentlyActive: true};
            if (isPlayerInUmbrella)
            {
                return;
            }

            var isPlayerHeadbuttCharging = selectedTalent is {TalentType: TalentType.Headbutt, IsCurrentlyActive: true};
            if (isPlayerHeadbuttCharging)
            {
                return;
            }

            var isPlayerRock = selectedTalent is {TalentType: TalentType.Rock, IsCurrentlyActive: true};
            if (isPlayerRock)
            {
                return;
            }
            
            var isPlayerIdle = playerSpaceshipState.Transform.Velocity.Length() < _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.TurnEngineOnWhenReachVelocity;
            if (isPlayerIdle)
            {
                playerSpaceshipState.IsEngineOn = true;
            }
        }

        public void TryAddEngineForceToPlayer(PlayerSpaceshipStateS2C playerSpaceshipState, float deltaTIme)
        {
            if (!playerSpaceshipState.IsEngineOn || !playerSpaceshipState.IsAlive)
            {
                return;
            }

            var transformState = playerSpaceshipState.Transform;
            var playerMovementSpeed = transformState.Velocity.Length();
            var targetMovementSpeed = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.TargetMovementSpeed;
            var isBelowTargetMovementSpeed = playerMovementSpeed < targetMovementSpeed;

            if (!isBelowTargetMovementSpeed)
            {
                return;
            }
            
            var engineForce = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.EngineAcceleration * deltaTIme * transformState.Direction;
            transformState.Velocity += engineForce;
            var velocityLength = transformState.Velocity.Length();
            var newSpeed = Mathf.Clamp(velocityLength, 0, targetMovementSpeed);
            transformState.Velocity = transformState.Velocity / velocityLength * newSpeed;
            playerSpaceshipState.Transform = transformState;
            
            // var transformState = playerSpaceshipState.Transform;
            // var targetMovementSpeed = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.TargetMovementSpeed;
            // var lookDirection = transformState.Direction;
            // var currentForwardSpeed = System.Numerics.Vector2.Dot(transformState.Velocity, lookDirection);
            // var isBelowTargetMovementSpeed = currentForwardSpeed < targetMovementSpeed;
            // if (!isBelowTargetMovementSpeed)
            // {
            //     return;
            // }
            //
            // var engineForce = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.EngineAcceleration * deltaTIme * lookDirection;
            // transformState.Velocity += engineForce;
            // playerSpaceshipState.Transform = transformState;
        }
    }
}
