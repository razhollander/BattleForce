using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService
{
    public interface IClientsNetworkDataService
    {
        Dictionary<long, ClientNetworkData> ClientsNetworkDataDictionary { get; }
        void AddClient(long clientId, bool isConnected);
        bool WasClientAtAnyTimeConnected(long clientId);
        void SetIsClientCurrentlyConnected(long clientId, bool isConnected);
        bool IsClientConnected(long clientId);
        void AssignPlayerToClient(long clientId, ushort playerId);
        void RemoveClient(long clientId);
    }
}