// using System.Numerics;
// using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
// using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
// using Core.Scripts.Extensions;
// using CoreDomain.Scripts.Services.CommandFactory;
// using CoreDomain.Scripts.Services.Logger.Base;
//
// namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
// {
//     public class AddPlayerCommand: BaseCommand, ICommandVoid
//     {
//         private IServerNetworkManager _networkManager;
//         private IMatchDataService _matchDataService;
//         private SimulationGamePlayConfig _gamePlayConfig;
//         private IPhysicsSimulator _physicsSimulator;
//         private IPlayersTalentsManager _playersTalentsManager;
//         private INetEventsDataService _netEventsDataService;
//         private string _playerName;
//         private int _playerId;
//         private int _playerTeamId;
//
//         public AddPlayerCommand SetPlayerName(string playerName)
//         {
//             _playerName = playerName;
//             return this;
//         }
//         
//         public AddPlayerCommand SetPlayerId(int playerId)
//         {
//             _playerId = playerId;
//             return this;
//         } 
//         
//         public AddPlayerCommand SetPlayerTeamId(int playerTeamId)
//         {
//             _playerTeamId = playerTeamId;
//             return this;
//         }
//
//         public override void ResolveDependencies()
//         {
//             _networkManager = _diContainer.Resolve<IServerNetworkManager>();
//             _matchDataService = _diContainer.Resolve<IMatchDataService>();
//             _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
//             _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
//             _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
//             _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
//         }
//
//         public void Execute()
//         {
//             var startingDirection = Simulation.Scripts.RNG.RNG.NextFloat(0, 360).AngleToVector();
//             var velocity = startingDirection * _gamePlayConfig.PlayerSpaceship.MovementSpeed;
//             var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
//             var health = _gamePlayConfig.PlayerSpaceship.StartHealth;
//             var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
//             var position = Vector2.One;
//             var playersAmount = _matchDataService.SimulationState.Players.Count;
//             var playerColor = _gamePlayConfig.PlayerSpaceship.PlayerColors[playersAmount % _gamePlayConfig.PlayerSpaceship.PlayerColors.Length];
//             var playerState = _matchDataService.AddPlayer(_playerId, _playerTeamId, _playerName, position, startingDirection, velocity, radius, health, shootCooldown, playerColor);
//             var peer = kvp.Key;
//             peer.Tag = playerId;
//             _physicsSimulator.AddPlayer(playerId, playerState.TeamId, position, startingDirection, radius);
//             _playersTalentsManager.AddPlayer(playerId);
//             _networkManager.AddPlayerPeer(playerId, peer);
//             _netEventsDataService.StartSavingPlayerEvents(playerId);
//             LogService.LogTopic("Processed player joined: " + playerState.ToJson(), LogTopicType.ServerNetwork);
//         }
//     }
// }