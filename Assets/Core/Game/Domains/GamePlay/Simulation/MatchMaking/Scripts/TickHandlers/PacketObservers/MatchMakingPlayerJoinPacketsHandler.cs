using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TickHandlers.PacketObservers
{
    public class MatchMakingPlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchMakingDataService _matchDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly ISimulationInputService _simulationInputService;
        private readonly IClientsNetworkDataService _clientsNetworkDataService;
        private readonly CapacityDict<NetPeer, JoinRequestPacketC2S> _playerJoinedPacketsPerPeer;
        private readonly ConcurrentPool<JoinRequestPacketC2S> _joinedRequestPacketsPool;
        private readonly ConcurrentPool<JoinResponsePacketS2C> _joinedResponsePacketsPool;
        private readonly NetworkConfig _networkConfig;
        public PacketTypeC2S PacketType => PacketTypeC2S.JoinRequest;

        public MatchMakingPlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchMakingDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator,
            INetEventsDataService netEventsDataService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ISimulationInputService simulationInputService, IClientsNetworkDataService clientsNetworkDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _simulationInputService = simulationInputService;
            _clientsNetworkDataService = clientsNetworkDataService;
            _networkConfig = networkConfig;
            _playerJoinedPacketsPerPeer = new CapacityDict<NetPeer, JoinRequestPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _joinedRequestPacketsPool = new ConcurrentPool<JoinRequestPacketC2S>(() => new JoinRequestPacketC2S(networkConfig.MaxCap.ConcurrentPlayers), networkConfig.MaxCap.JoinRequestPackets);
            _joinedResponsePacketsPool = new ConcurrentPool<JoinResponsePacketS2C>(() => new JoinResponsePacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount), networkConfig.MaxCap.JoinRequestPackets);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void ProcessPlayersJoined(int processedTick)
        {
            var startingDirection = Vector2.One;
            var velocity = startingDirection * 0.01f;
            var radius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
            var heartRadius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.DefaultHeartRadius;
            var shootCooldown = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.ShootCooldown;

            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                var clientId = kvp.Value.ClientId;
                var peer = kvp.Key;
                var isClientConnected = _clientsNetworkDataService.IsClientConnected(clientId);
                var isMaxPlayers = _matchDataService.SimulationState.Players.Count == _networkConfig.MaxCap.ConcurrentPlayers;
                var joinResponse = _joinedResponsePacketsPool.Get();
                joinResponse.Clear();
                if (isClientConnected || isMaxPlayers)
                {
                    LogService.LogError($"Cant join player because: isMaxPlayers: {isMaxPlayers}, isClientConnected: {isClientConnected}");
                    joinResponse.IsSuccess = false;
                }
                else
                {
                    joinResponse.IsSuccess = true;
                    joinResponse.IsMatchMaking = true;
                    joinResponse.MatchMakingSimulationState = _matchDataService.SimulationState;
                    joinResponse.OccuredOnTick = processedTick;
                    _clientsNetworkDataService.AddClient(clientId, true);
                    peer.Tag = clientId;
                    var playersConnected = new List<MatchMakingPlayerStateS2C>(kvp.Value.PlayerJoinedList.Count);

                    foreach (var playerJoined in kvp.Value.PlayerJoinedList.AsSpan())
                    {
                        var playerTeamId = (ushort) (_matchDataService.SimulationState.Players.Count % _sharedGamePlayConfig.MaxTeamsAmount + 1);
                        var position = DonutQuadrantWalls.GetTeamFloorCenter(_sharedGamePlayConfig.TeamIds, playerTeamId, _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsRadius);
                        var playerState = _matchDataService.AddPlayer(playerJoined.PlayerName, position, startingDirection, velocity, radius, shootCooldown, playerTeamId);
                        var playerId = playerState.Id;
                        joinResponse.PlayerIdToDeviceIdDictionary.Add(playerId, playerJoined.InputDeviceId);
                        _simulationInputService.AddPlayer(playerId);
                        _physicsSimulator.AddPlayer(playerId, playerState.TeamId, position, startingDirection, radius, heartRadius);
                        _clientsNetworkDataService.AssignPlayerToClient(clientId, playerId);
                        playersConnected.Add(playerState);
                    }
                    
                    _networkManager.AddClientPeer(clientId, peer);
                    _netEventsDataService.StartSavingClientEvents(clientId);
                    _netEventsDataService.AddMatchMakingClientJoinAcceptedEvent(processedTick, playersConnected, _matchDataService.SimulationState, clientId);
                }
                
                _networkManager.SendPacketToPeerSerialized(peer, PacketTypeS2C.JoinResponse, joinResponse, DeliveryMethod.ReliableOrdered);
                LogService.LogTopic("Processed player joined: "+clientId, LogTopicType.ServerNetwork);
            }

            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                _joinedRequestPacketsPool.Return(kvp.Value);
            }

            _playerJoinedPacketsPerPeer.Clear();
        }

        public void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback)
        {
            var newPacket = _joinedRequestPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnJoinReceived(newPacket, peer);
        }

        private void OnJoinReceived(JoinRequestPacketC2S joinRequestPacket, NetPeer peer)
        {
            LogService.LogTopic("Join packet received: " + peer.Id, LogTopicType.ServerNetwork);
            _playerJoinedPacketsPerPeer.Add(peer, joinRequestPacket);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }
    }
}