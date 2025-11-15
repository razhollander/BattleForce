using System;
using CoreDomain.Scripts.Services.Logger.Base;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Game.Domains.GamePlay.Both.NetworkManager
{
    public class BFNetworkManager : IBattleForceNetworkManager
    {
        public int ServerTickRate = 60;
        public bool ShouldDisconnectInactiveConnectionsAfterTimeout;
        public float DisconnectInactiveTimeoutInSeconds = 60f;
        public bool ShouldDisconnectAfterAnyException = false;
        public int MaxConcurrentConnections = 8;

        // transport layer
        [Header("Network Info")]
        [Tooltip("Transport component attached to this object that server and client will use to connect")]
        public Transport transport;

        // helper enum to know if we started the networkmanager as server/client/host.
        // -> this is necessary because when StartHost changes server scene to
        //    online scene, FinishLoadScene is called and the host client isn't
        //    connected yet (no need to connect it before server was fully set up).
        //    in other words, we need this to know which mode we are running in
        //    during FinishLoadScene.
        public NetworkManagerMode NetworkMode { get; private set; }
        
         /// <summary>Starts a network "host" - a server and client in the same application.</summary>
        public void StartHost()
        {
            if (NetworkServer.active || NetworkClient.active)
            {
                Debug.LogWarning("Server or Client already started.");
                return;
            }

            NetworkMode = NetworkManagerMode.Host;
            SetupServer();
            FinishStartHost();
            
        }

        // This may be set true in StartHost and is evaluated in FinishStartHost
        bool finishStartHostPending;
        private BFNetworkServerMessagesListener _BFNetworkServerMessagesListener;

        // FinishStartHost is guaranteed to be called after the host server was
        // fully started and all the asynchronous StartHost magic is finished
        // (= scene loading), or immediately if there was no asynchronous magic.
        //
        // note: we don't really need FinishStartClient/FinishStartServer. the
        //       host version is enough.
        void FinishStartHost()
        {
            NetworkClient.ConnectHost();
            OnStartServer();
            OnStartHost();
            SetupClient();
            RegisterClientMessages();
            HostMode.InvokeOnConnected();
            OnStartClient();
        }
        
        void SetupServer()
        {
            NetworkServer.disconnectInactiveConnections = ShouldDisconnectInactiveConnectionsAfterTimeout;
            NetworkServer.disconnectInactiveTimeout = DisconnectInactiveTimeoutInSeconds;
            NetworkServer.exceptionsDisconnect = ShouldDisconnectAfterAnyException;
            _BFNetworkServerMessagesListener = new BFNetworkServerMessagesListener();
            // if (authenticator != null) // no authentication currently
            // {
            //     authenticator.OnStartServer();
            //     authenticator.OnServerAuthenticated.AddListener(OnServerAuthenticated);
            // }
            
            NetworkServer.Listen(MaxConcurrentConnections);
            _BFNetworkServerMessagesListener.RegisterServerMessages();
        }
    }

    public interface IBattleForceNetworkManager
    {
        public void StartHost();
    }
}
