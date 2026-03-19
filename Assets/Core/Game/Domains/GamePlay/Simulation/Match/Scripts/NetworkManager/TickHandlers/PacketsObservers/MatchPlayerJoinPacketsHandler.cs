using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Extensions;
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
        private readonly CapacityDict<NetPeer, JoinRequestPacketC2S> _playerRejoinedPacketsPerPeer;
        private readonly ConcurrentPool<JoinRequestPacketC2S> _joinedRequestPacketsPool;
        private readonly ConcurrentPool<JoinResponsePacketS2C> _joinedResponsePacketsPool;

        public PacketTypeC2S PacketType => PacketTypeC2S.JoinRequest;

        public MatchPlayerJoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            INetEventsDataService iNetEventsDataService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ISimulationInputService simulationInputService)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _netEventsDataService = iNetEventsDataService;
            _simulationInputService = simulationInputService;
            _playerRejoinedPacketsPerPeer = new CapacityDict<NetPeer, JoinRequestPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _joinedRequestPacketsPool = new ConcurrentPool<JoinRequestPacketC2S>(() => new JoinRequestPacketC2S(), networkConfig.MaxCap.JoinRequestPackets);
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
                var playerName = kvp.Value.PlayerName;
                var isPlayerAlreadyInMatch = _matchDataService.SimulationState.TryGetPlayerByName(playerName, out var existingPlayerState);
                var joinResponse = _joinedResponsePacketsPool.Get();
                var peer = kvp.Key;

                joinResponse.Clear();
                if (isPlayerAlreadyInMatch && !existingPlayerState.IsConnected)
                {
                    existingPlayerState.IsConnected = true;
                    joinResponse.IsSuccess = true;
                    joinResponse.IsMatchMaking = false;
                    var playerId = existingPlayerState.Id;
                    joinResponse.LocalPlayerId = playerId;
                    joinResponse.MatchSimulationState = _matchDataService.SimulationState;
                    joinResponse.OccuredOnTick = processedTick;
                    peer.Tag = playerId;
                    
                    _simulationInputService.AddPlayer(playerId);
                    _networkManager.AddPlayerPeer(playerId, peer);
                    _netEventsDataService.StartSavingPlayerEvents(playerId);
                    var nw = new NetDataWriter(true);
                    var nr = new NetDataReader();
                    existingPlayerState.Serialize(nw);
                    nr.SetSource(nw);
                    var playerStateCopy = new PlayerStateS2C(3);
                    playerStateCopy.Deserialize(nr);
                    _netEventsDataService.AddPlayerJoinAcceptedEvent(processedTick, playerStateCopy, _matchDataService.SimulationState);
                }
                else
                {
                    var isAlreadyConnected = isPlayerAlreadyInMatch && existingPlayerState.IsConnected;
                    LogService.LogError($"Can't join server isPlayerAlreadyInMatch {isPlayerAlreadyInMatch}, IsConnected {isAlreadyConnected}, playerName {playerName}");
                    joinResponse.IsSuccess = false;
                }
                _networkManager.SendPacketToPeerSerialized(peer, PacketTypeS2C.JoinResponse, joinResponse, DeliveryMethod.ReliableOrdered);

                LogService.LogTopic("Processed player rejoined: " + playerName, LogTopicType.ServerNetwork);
            }

            foreach (var kvp in _playerRejoinedPacketsPerPeer)
            {
                _joinedRequestPacketsPool.Return(kvp.Value);
            }
            _playerRejoinedPacketsPerPeer.Clear();
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
            _playerRejoinedPacketsPerPeer.Add(peer, joinRequestPacket);
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);
        }
    }
}