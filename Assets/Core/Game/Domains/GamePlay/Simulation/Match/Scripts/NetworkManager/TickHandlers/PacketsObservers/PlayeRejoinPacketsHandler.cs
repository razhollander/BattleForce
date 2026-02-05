using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
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
    public class PlayeRejoinPacketsHandler : IPlayeRejoinPacketsHandler
    {
        private readonly IServerNetworkManager _networkManager;
        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly CapacityDict<NetPeer, JoinRequestPacketC2S> _playerRejoinedPacketsPerPeer;
        private readonly ConcurrentPool<JoinRequestPacketC2S> _joinedRequestPacketsPool;
        public PacketTypeC2S PacketType => PacketTypeC2S.MatchRejoinRequest;

        public PlayeRejoinPacketsHandler(IServerNetworkManager networkManager, IMatchDataService matchDataService,
            INetEventsDataService iNetEventsDataService, NetworkConfig networkConfig)
        {
            _networkManager = networkManager;
            _matchDataService = matchDataService;
            _netEventsDataService = iNetEventsDataService;
            _playerRejoinedPacketsPerPeer = new CapacityDict<NetPeer, JoinRequestPacketC2S>(networkConfig.MaxCap.ConcurrentPlayers);
            _joinedRequestPacketsPool = new ConcurrentPool<JoinRequestPacketC2S>(() => new JoinRequestPacketC2S(), networkConfig.MaxCap.JoinRequestPackets);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);
        }

        public void ProcessPlayersRejoined(int processedTick)
        {
            foreach (var kvp in _playerRejoinedPacketsPerPeer)
            { 
                var playerName = kvp.Value.PlayerName;
                var playerState = _matchDataService.SimulationState.GetPlayerByName(playerName);
                var peer = kvp.Key;
                peer.Tag = playerState.Id;
                
                _netEventsDataService.AddPlayerRejoinAcceptedEvent(processedTick, playerState, _matchDataService.SimulationState);
                LogService.LogTopic("Processed player rejoined: " + playerState.ToJson(), LogTopicType.ServerNetwork);
            }

            foreach (var kvp in _playerRejoinedPacketsPerPeer)
            {
                _joinedRequestPacketsPool.Return(kvp.Value);
            }
            _playerRejoinedPacketsPerPeer.Clear();
        }
        
        public void OnPacketReceived(NetDataReader reader, NetPeer peer)
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