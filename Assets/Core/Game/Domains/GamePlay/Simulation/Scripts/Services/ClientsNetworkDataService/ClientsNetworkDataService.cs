using System.Collections.Generic;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService
{
    public class ClientsNetworkDataService : IClientsNetworkDataService
    {
        public Dictionary<long, ClientNetworkData> ClientsNetworkDataDictionary { get;private set; }
        private readonly ConcurrentPool<ClientNetworkData> _clientNetworkDataPool;
        public ClientsNetworkDataService(NetworkConfig networkConfig)
        {
            _clientNetworkDataPool= new ConcurrentPool<ClientNetworkData>(() => new ClientNetworkData(networkConfig.MaxCap.ConcurrentPlayers), networkConfig.MaxCap.ConcurrentPlayers);
            ClientsNetworkDataDictionary = new Dictionary<long, ClientNetworkData>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void AddClient(long clientId, bool isConnected)
        {
            if (ClientsNetworkDataDictionary.ContainsKey(clientId))
            {
                LogService.LogError($"Client already exists: {clientId}");
                return;
            }
            
            ClientsNetworkDataDictionary.Add(clientId, _clientNetworkDataPool.Get());
            ClientsNetworkDataDictionary[clientId].IsConnected = isConnected;
        }
        
        public void SetIsClientCurrentlyConnected(long clientId, bool isConnected)
        {
            if (!ClientsNetworkDataDictionary.TryGetValue(clientId, out var clientNetworkData))
            {
                LogService.LogError($"Client doesn't exists: {clientId}");
                return;
            }

            clientNetworkData.IsConnected = isConnected;
        }

        public bool WasClientAtAnyTimeConnected(long clientId)
        {
            return ClientsNetworkDataDictionary.TryGetValue(clientId, out var _);
        }
        
        public bool IsClientConnected(long clientId)
        {
            if (!ClientsNetworkDataDictionary.TryGetValue(clientId, out var clientNetworkData))
            {
                return false;
            }

            return clientNetworkData.IsConnected;
        }

        public void AssignPlayerToClient(long clientId, ushort playerId)
        {
            if (!ClientsNetworkDataDictionary.ContainsKey(clientId))
            {
                LogService.LogError($"Client doesn't exist: {clientId}");
                return;
            }

            ref var playerIds = ref ClientsNetworkDataDictionary[clientId].PlayerIds.AddAndGet();
            playerIds = playerId;
        }

        public bool IsPlayerAssignedToClient(long clientId, ushort playerId)
        {
            if (!ClientsNetworkDataDictionary.TryGetValue(clientId, out var clientNetworkData))
            {
                return false;
            }
            
            return clientNetworkData.PlayerIds.Contains(playerId);
        }

        public void RemoveClient(long clientId)
        {
            if (!ClientsNetworkDataDictionary.ContainsKey(clientId))
            {
                LogService.LogError($"Client doesn't exist: {clientId}");
                return;
            }

            ClientsNetworkDataDictionary.Remove(clientId);
        }

        public bool IsClientExist(long clientId)
        {
            return ClientsNetworkDataDictionary.ContainsKey(clientId);
        }
    }
}