using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
// using System.Collections.Generic;
// using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
// using Core.Game.Domains.GamePlay.Shared.NetworkManager;
// using Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
// using Core.Scripts.Extensions;
// using Core.Scripts.Network;
// using CoreDomain.Scripts.Extensions;
// using CoreDomain.Scripts.Services.Logger.Base;
// using LiteNetLib;
//
// namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers
// {
//     public class PlayerInputsPacketsHandler : IPlayerInputsPacketsHandler
//     {
//         private readonly IServerNetworkManager _networkManager;
//         private readonly IMatchDataService _matchDataService;
//         private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
//         private readonly NetworkConfig _networkConfig;
//         private readonly Dictionary<int, Dictionary<int, PlayerInputPacketC2S>> _inputsByTick = new ();
//         private readonly Dictionary<int,PlayerInputPacketC2S> _cachedLastProcessedInput = new ();
//
//         public PlayerInputsPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig)
//         {
//             _networkManager = networkManager;
//             _matchDataService = matchDataService;
//             _gamePlayConfigService = gamePlayConfigService;
//             _networkConfig = networkConfig;
//         }
//
//         public void RegisterListeners()
//         {
//             _networkManager.SubscribeNetSerializable<PlayerInputPacketC2S>(OnPlayerInputReceived);
//         }
//
//         public void InitExitPoint()
//         {
//             _networkManager.RemoveSubscription<PlayerInputPacketC2S>();
//         }
//
//         public Dictionary<int, PlayerInputPacketC2S> ProcessInputsInTick(int tick)
//         {
//             RemoveInputsWithSmallerTick(tick);
//             
//             if (!_inputsByTick.TryGetValue(tick, out var inputsInTick))
//             {
//                 inputsInTick = new Dictionary<int, PlayerInputPacketC2S>();
//                 _inputsByTick.Add(tick, inputsInTick);
//             }
//
//             for (var i = 0; i < _matchDataService.SimulationState.PlayersCount; i++)
//             {
//                 var player = _matchDataService.SimulationState.Players[i];
//                 var playerId = player.Id;
//                 if (!TryGetInputForPlayerInTick(tick, playerId, out var playerInputPacket))
//                 {
//                     if (!TryGetCachedInputForPlayer(playerId, out playerInputPacket))
//                     {
//                         LogService.LogTopic(
//                             $"Didn't find any last cached inputs for player {playerId}! for tick {tick}",
//                             LogTopicType.ServerNetwork);
//                         continue;
//                     }
//
//                     inputsInTick.Add(playerId, playerInputPacket);
//                     LogService.LogTopic($"Using last cached inputs for player {playerId}! for tick {tick}",
//                         LogTopicType.ServerNetwork);
//                 }
//
//                 var playerModel = _matchDataService.GetPlayer(playerId);
//                 var rotationDelta = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.RotationSpeed * _networkConfig.DeltaTime;
//                 var rotationAngle =
//                     (playerInputPacket.IsMoveLeftInputPressed.ToInt() -
//                      playerInputPacket.IsMoveRightInputPressed.ToInt()) * rotationDelta;
//                 var rotatedVector = playerModel.Spaceship.Transform.RotationVector.Rotate(rotationAngle);
//                 LogService.LogTopic($"rotatedVector {rotatedVector} rotationAngle {rotationAngle} playerInputPacket.IsMoveLeftInputPressed {playerInputPacket.IsMoveLeftInputPressed} playerInputPacket.IsMoveRightInputPressed.ToInt() {playerInputPacket.IsMoveRightInputPressed} rotationDelta {rotationDelta}");
//                 playerModel.Spaceship.Transform.RotationVector = rotatedVector;
//                 playerModel.Spaceship.Transform.Position += playerModel.Spaceship.Transform.RotationVector *
//                                                             _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.MovementSpeed *
//                                                             _networkConfig.DeltaTime;
//                 _matchDataService.SetPlayer(playerId, playerModel);
//                 _cachedLastProcessedInput[playerId] = playerInputPacket;
//             }
//
//             _inputsByTick.Remove(tick);
//             return inputsInTick;
//         }
//         
//         private void RemoveInputsWithSmallerTick(int maxTickExclusive)
//         {
//             var ticksToRemove = new List<int>();
//             
//             foreach (var kvp in _inputsByTick)
//             {
//                 var tickOfInputs = kvp.Key;
//                 if (tickOfInputs < maxTickExclusive)
//                 {
//                     ticksToRemove.Add(tickOfInputs);
//                     LogService.LogTopic($"Remove inputs of tick {tickOfInputs}", LogTopicType.ServerNetwork);
//                 }
//             }
//             
//             foreach (var tickToRemove in ticksToRemove)
//             {
//                 _inputsByTick.Remove(tickToRemove);
//             }
//         }
//
//         private bool TryGetCachedInputForPlayer(int playerId, out PlayerInputPacketC2S playerInputPacket)
//         {
//             return _cachedLastProcessedInput.TryGetValue(playerId, out playerInputPacket);
//         }
//
//         private void OnPlayerInputReceived(PlayerInputPacketC2S playerInputPacket, NetPeer peer)
//         {
//             var tick = playerInputPacket.Tick;
//             var playerId = (int)peer.Tag;
//             _inputsByTick.TryAdd(tick, new Dictionary<int, PlayerInputPacketC2S>());
//             if (!_inputsByTick[tick].TryAdd(playerId, playerInputPacket))
//             {
//                 _inputsByTick[tick][playerId] = playerInputPacket;
//             }
//             LogService.LogTopic("Input packet received from player id" + playerId, LogTopicType.ServerNetwork);
//         }
//
//         private bool TryGetInputForPlayerInTick(int tick, int playerId, out PlayerInputPacketC2S playerInputPacket)
//         {
//             if (_inputsByTick[tick].ContainsKey(playerId))
//             {
//                 playerInputPacket = _inputsByTick[tick][playerId];
//                 return true;
//             }
//
//             playerInputPacket = default;
//             return false;
//         }
//     }
// }