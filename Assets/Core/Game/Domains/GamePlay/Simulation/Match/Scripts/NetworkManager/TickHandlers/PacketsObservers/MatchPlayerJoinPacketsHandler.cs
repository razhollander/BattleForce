using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public class MatchPlayerJoinPacketsHandler : IMatchPlayerJoinPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly ISimulationInputService _simulationInputService;
        private readonly IClientsNetworkDataService _clientsNetworkDataService;
        private readonly CapacityDict<NetPeer, JoinRequestPacketC2S> _playerRejoinedPacketsPerPeer;
        private readonly ConcurrentPool<JoinRequestPacketC2S> _joinedRequestPacketsPool;
        private readonly ConcurrentPool<JoinResponsePacketS2C> _joinedResponsePacketsPool;

        public PacketTypeC2S PacketType => PacketTypeC2S.JoinRequest;

        public MatchPlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            INetEventsDataService netEventsDataService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ISimulationInputService simulationInputService, IClientsNetworkDataService clientsNetworkDataService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _simulationInputService = simulationInputService;
            _clientsNetworkDataService = clientsNetworkDataService;
            _playerRejoinedPacketsPerPeer = new CapacityDict<NetPeer, JoinRequestPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _joinedRequestPacketsPool = new ConcurrentPool<JoinRequestPacketC2S>(() => new JoinRequestPacketC2S(networkConfig.MaxCap.ConcurrentPlayers), networkConfig.MaxCap.JoinRequestPackets);
            _joinedResponsePacketsPool = new ConcurrentPool<JoinResponsePacketS2C>(() => new JoinResponsePacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount), networkConfig.MaxCap.JoinRequestPackets);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void ProcessPlayersJoined(int processedTick)
        {
            foreach (var kvp in _playerRejoinedPacketsPerPeer)
            {
                var joinResponse = _joinedResponsePacketsPool.Get();
                joinResponse.Clear();
                var clientId = kvp.Value.ClientId;
                var isClientConnected = _clientsNetworkDataService.IsClientConnected(clientId);
                var wasClientAtAnyTimeConnected = _clientsNetworkDataService.WasClientAtAnyTimeConnected(clientId);
                var isReconnect = wasClientAtAnyTimeConnected && !isClientConnected;
                joinResponse.IsMatchMaking = false; 
                
                if (isReconnect)
                {
                    joinResponse.IsSuccess = true;
                    var peer = kvp.Key;
                    peer.Tag = clientId;
                    var playersReconnected = new List<PlayerStateS2C>(kvp.Value.PlayerJoinedList.Count);
                    joinResponse.MatchSimulationState = _matchDataService.SimulationState;
                    joinResponse.OccuredOnTick = processedTick;
                    _networkManager.AddClientPeer(clientId, peer);
                    _clientsNetworkDataService.SetIsClientCurrentlyConnected(clientId, true);
                    _netEventsDataService.StartSavingClientEvents(clientId);
                    //_netEventsDataService.StartSavingClientEvents(playerId);

                    foreach (var playerJoined in kvp.Value.PlayerJoinedList.AsSpan())
                    {
                        var playerName = playerJoined.PlayerName;
                        _matchDataService.SimulationState.TryGetPlayerByName(playerName, out var existingPlayerState);
                        var playerId = existingPlayerState.Id;
                        playersReconnected.Add(existingPlayerState);
                        joinResponse.PlayerIdToDeviceIdDictionary.Add(playerId, playerJoined.InputDeviceId);
                        _simulationInputService.AddPlayer(playerId);
                        LogService.LogTopic("Processed player rejoined: " + playerName, LogTopicType.ServerNetwork);
                    }
                    
                    _netEventsDataService.AddClientJoinAcceptedEvent(processedTick, playersReconnected, _matchDataService.SimulationState, clientId);
                    _networkManager.SendPacketToPeerSerialized(peer, PacketTypeS2C.JoinResponse, joinResponse, DeliveryMethod.ReliableOrdered);

                }
                else
                {
                    joinResponse.IsSuccess = false;
                    LogService.LogError($"Can't join server wasClientAtAnyTimeConnected {wasClientAtAnyTimeConnected}, IsConnected {isClientConnected}, client id {clientId}");
                }
            }

            foreach (var kvp in _playerRejoinedPacketsPerPeer)
            {
                _joinedRequestPacketsPool.Return(kvp.Value);
            }
            _playerRejoinedPacketsPerPeer.Clear();
        }

        // private PlayerStateS2C CopyPlayerState(PlayerStateS2C playerState)
        // {
        //     var nw = new NetDataWriter(true);
        //     var nr = new NetDataReader();
        //     playerState.Serialize(nw);
        //     nr.SetSource(nw);
        //     var playerStateCopy = new PlayerStateS2C(_sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, _networkConfig.MaxCap.ConcurrentPlayers - 1);
        //     playerStateCopy.Deserialize(nr);
        //     return playerStateCopy;
        // }
        
        public void OnPacketReceived(NetDataReader reader, NetPeer peer, bool isReceivedFromPlayback)
        {
            var newPacket = _joinedRequestPacketsPool.Get();
            newPacket.Deserialize(reader);
            OnJoinReceived(newPacket, peer);
        }
        
        private void OnJoinReceived(JoinRequestPacketC2S joinRequestPacket, NetPeer peer)
        {
            LogService.LogTopic("Join packet received, peer.Id: " + peer.Id, LogTopicType.ServerNetwork);
            _playerRejoinedPacketsPerPeer.Add(peer, joinRequestPacket);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }
    }
}