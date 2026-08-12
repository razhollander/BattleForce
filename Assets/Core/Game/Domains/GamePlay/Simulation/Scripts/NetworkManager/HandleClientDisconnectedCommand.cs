using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class HandleClientDisconnectedCommand : BaseCommand, ICommandVoid
    {
        private IClientsNetworkDataService _clientsNetworkDataService;
        private INetEventsDataService _netEventsDataService;
        private IServerNetworkManager _serverNetworkManager;

        private long _clientId;

        public HandleClientDisconnectedCommand SetClientId(long clientId)
        {
            _clientId = clientId;
            return this;
        }

        public override void ResolveDependencies()
        {
            _clientsNetworkDataService = _diContainer.Resolve<IClientsNetworkDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
        }

        public void Execute()
        {
            if (!_clientsNetworkDataService.IsClientConnected(_clientId))
            {
                LogService.LogError($"Disconnected client that is not marked as connected: {_clientId}");
                return;
            }

            _clientsNetworkDataService.SetIsClientCurrentlyConnected(_clientId, false);
            _netEventsDataService.StopSavingClientEvents(_clientId);
            _serverNetworkManager.RemoveClientPeer(_clientId);
        }
    }
}
