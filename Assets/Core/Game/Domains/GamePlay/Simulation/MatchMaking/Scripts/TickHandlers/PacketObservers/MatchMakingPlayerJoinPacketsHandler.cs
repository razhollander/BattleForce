using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
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
        private readonly CapacityDict<NetPeer, FixedClassUnorderedList<JoinRequestPacketC2S>> _playerJoinedPacketsPerPeer;
        private readonly ConcurrentPool<FixedClassUnorderedList<JoinRequestPacketC2S>> _playerJoinedPacketsListPool;
        private readonly ConcurrentPool<JoinRequestPacketC2S> _joinedRequestPacketsPool;
        private readonly ConcurrentPool<JoinResponsePacketS2C> _joinedResponsePacketsPool;
        private readonly NetworkConfig _networkConfig;
        public PacketTypeC2S PacketType => PacketTypeC2S.JoinRequest;

        public MatchMakingPlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchMakingDataService matchDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, IPhysicsSimulator physicsSimulator,
            INetEventsDataService netEventsDataService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ISimulationInputService simulationInputService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _simulationInputService = simulationInputService;
            _networkConfig = networkConfig;
            _playerJoinedPacketsPerPeer = new CapacityDict<NetPeer, FixedClassUnorderedList<JoinRequestPacketC2S>>(networkConfig.MaxCap.ConcurrentPlayers);
            _playerJoinedPacketsListPool = new ConcurrentPool<FixedClassUnorderedList<JoinRequestPacketC2S>>(() => new FixedClassUnorderedList<JoinRequestPacketC2S>(networkConfig.MaxCap.JoinRequestPackets, () => new JoinRequestPacketC2S()), networkConfig.MaxCap.ConcurrentPlayers);
            _joinedRequestPacketsPool = new ConcurrentPool<JoinRequestPacketC2S>(() => new JoinRequestPacketC2S(), networkConfig.MaxCap.JoinRequestPackets);
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
                var peer = kvp.Key;
                foreach (var packet in kvp.Value.AsSpan())
                {
                    var playerName = packet.PlayerName;

                    var isPlayerAlreadyInMatch = _matchDataService.SimulationState.TryGetPlayerByName(playerName, out _);
                    var isMaxPlayers = _matchDataService.SimulationState.Players.Count == _networkConfig.MaxCap.ConcurrentPlayers;

                    var joinResponse = _joinedResponsePacketsPool.Get();
                    joinResponse.Clear();
                    if (isPlayerAlreadyInMatch || isMaxPlayers)
                    {
                        LogService.LogError($"Cant join player because: isMaxPlayers: {isMaxPlayers}, isPlayerAlreadyInMatch: {isPlayerAlreadyInMatch}");
                        joinResponse.IsSuccess = false;
                    }
                    else
                    {
                        joinResponse.IsSuccess = true;
                        joinResponse.IsMatchMaking = true;
                        var playerTeamId = (ushort) (_matchDataService.SimulationState.Players.Count % _sharedGamePlayConfig.MaxTeamsAmount + 1);
                        var position = DonutQuadrantWalls.GetTeamFloorCenter(_sharedGamePlayConfig.TeamIds, playerTeamId, _sharedGamePlayConfig.MatchMakingEnvironment.TeamFloorsRadius);
                        var playerState = _matchDataService.AddPlayer(playerName, position, startingDirection, velocity, radius, shootCooldown, playerTeamId);
                        var playerId = playerState.Id;
                        joinResponse.LocalPlayerId = playerId;
                        joinResponse.MatchMakingSimulationState = _matchDataService.SimulationState;
                        joinResponse.OccuredOnTick = processedTick;
                        peer.Tag = playerId;

                        _simulationInputService.AddPlayer(playerId);
                        _physicsSimulator.AddPlayer(playerId, playerState.TeamId, position, startingDirection, radius, heartRadius);
                        _networkManager.AddPlayerPeer(playerId, peer);
                        _netEventsDataService.StartSavingPlayerEvents(playerId);
                        _netEventsDataService.AddMatchMakingPlayerJoinAcceptedEvent(processedTick, playerState, _matchDataService.SimulationState);
                    }
                    
                    _networkManager.SendPacketToPeerSerialized(peer, PacketTypeS2C.JoinResponse, joinResponse, DeliveryMethod.ReliableOrdered);

                    LogService.LogTopic("Processed player joined: "+playerName, LogTopicType.ServerNetwork);
                }
            }

            foreach (var kvp in _playerJoinedPacketsPerPeer)
            {
                kvp.Value.Clear();
                _playerJoinedPacketsListPool.Return(kvp.Value);
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
            LogService.LogTopic("Join packet received: " + joinRequestPacket.PlayerName, LogTopicType.ServerNetwork);
            if (!_playerJoinedPacketsPerPeer.TryGetValue(peer, out var list))
            {
                list = _playerJoinedPacketsListPool.Get();
                _playerJoinedPacketsPerPeer.Add(peer, list);
            }

            var packet = list.AddAndGet();
            packet.Deserialize(new NetDataReader(new NetDataWriter().Put(joinRequestPacket.PlayerName).Put(joinRequestPacket.IsGamePadEnabled).Data));
            // Return original since we cloned it conceptually into the class list
            _joinedRequestPacketsPool.Return(joinRequestPacket);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }
    }
}