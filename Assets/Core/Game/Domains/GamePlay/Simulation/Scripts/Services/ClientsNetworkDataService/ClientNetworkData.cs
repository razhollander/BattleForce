using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService
{
    public class ClientNetworkData
    {
        public long ClientId;
        public bool IsConnected;
        public readonly FixedUnorderedList<ushort> PlayerIds;

        public ClientNetworkData(int maxConcurrentPlayers)
        {
            IsConnected = true;
            PlayerIds = new FixedUnorderedList<ushort>(maxConcurrentPlayers);
        }
    }
}