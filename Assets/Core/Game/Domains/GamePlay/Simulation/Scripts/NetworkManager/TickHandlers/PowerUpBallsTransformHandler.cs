// using System;
// using System.Numerics;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
// using Core.Scripts.Network;
//
// namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers
// {
//     public class PowerUpBallsTransformHandler : IPowerUpBallsTransformHandler
//     {
//         private readonly IMatchDataService _matchDataService;
//         private readonly PowerUpsConfig _powerUpsConfig;
//         private readonly IPhysicsSimulator _physicsSimulator;
//
//         public PowerUpBallsTransformHandler(
//             IMatchDataService matchDataService,
//             PowerUpsConfig powerUpsConfig,
//             IPhysicsSimulator physicsSimulator)
//         {
//             _matchDataService = matchDataService;
//             _powerUpsConfig = powerUpsConfig;
//             _physicsSimulator = physicsSimulator;
//         }
//
//         public void UpdatePowerUpsTransform()
//         {
//             // Enforce constant speed logic for dynamic bodies
//              for (int i = 0; i < _matchDataService.SimulationState.PowerUpBalls.Count; i++)
//             {
//                 var powerUp = _matchDataService.SimulationState.PowerUpBalls[i];
//                 var body = _physicsSimulator.GetPowerUpBall(powerUp.Id);
//
//                 if (body != null)
//                 {
//                     var velocity = body.GetLinearVelocity();
//                     var speed = velocity.Length();
//
//                     // If speed is wrong (e.g. after collision or initial), reset it to Config Speed in the current direction
//                     // If speed is 0 (shouldn't happen if initialized correctly, but safety check)
//                     if (Math.Abs(speed - _powerUpsConfig.MoveSpeed) > 0.1f || speed == 0)
//                     {
//                         var dir = velocity;
//                         if (dir == Vector2.Zero)
//                         {
//                             dir = powerUp.Velocity;
//                         }
//
//                         dir = Vector2.Normalize(dir);
//                         body.SetLinearVelocity(dir * _powerUpsConfig.MoveSpeed);
//                     }
//
//                     if (speed > 0)
//                     {
//                         _matchDataService.SimulationState.PowerUpBalls.GetByIndex(i).Velocity = Vector2.Normalize(velocity);
//                     }
//                 }
//             }
//         }
//     }
// }
