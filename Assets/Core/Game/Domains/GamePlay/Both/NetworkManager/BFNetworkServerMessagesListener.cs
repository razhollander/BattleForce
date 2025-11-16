using System;
using CoreDomain.Scripts.Services.Logger.Base;
using Mirror;

namespace Core.Game.Domains.GamePlay.Both.NetworkManager
{
    public class BFNetworkServerMessagesListener
    {
        public void RegisterServerMessages()
        {
            NetworkServer.OnConnectedEvent = OnServerConnectInternal;
            NetworkServer.OnDisconnectedEvent = OnServerDisconnect;
            NetworkServer.OnErrorEvent = OnServerError;
            NetworkServer.OnTransportExceptionEvent = OnServerTransportException;
            NetworkServer.RegisterHandler<AddPlayerMessage>(OnServerAddPlayerInternal);

            // Network Server initially registers its own handler for this, so we replace it here.
            NetworkServer.ReplaceHandler<ReadyMessage>(OnServerReadyMessageInternal);
        }
        
        
        public void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            NetworkServer.DestroyPlayerForConnection(conn);
        }
        
        void OnServerConnectInternal(NetworkConnectionToClient conn)
        {
            conn.isAuthenticated = true;
            LogService.LogTopic("Server Authenticated", LogTopicType.Network);
        }

        public void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason)
        {
            LogService.LogError("Server had network error " + reason);
        }
        
        public void OnServerTransportException(NetworkConnectionToClient conn, Exception exception)
        {
            LogService.LogError("Server Transport had network error " + exception.Message);
        }
        
        void OnServerAddPlayerInternal(NetworkConnectionToClient conn, AddPlayerMessage msg)
        {
            if (conn.identity != null)
            {
                LogService.LogError("There is already a player for this connection.");
                return;
            }
            
            LogService.LogTopic("Add player", LogTopicType.Network);
            //NetworkServer.AddPlayerForConnection(conn, player);
        }
        
        void OnServerReadyMessageInternal(NetworkConnectionToClient conn, ReadyMessage msg)
        {
            LogService.LogTopic("Player Ready", LogTopicType.Network);
            conn.isReady = true;
            conn.Send(new ObjectSpawnStartedMessage());
            conn.Send(new ObjectSpawnFinishedMessage());
        }
    }
}