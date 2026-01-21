using System;
using System.Collections.Generic;
using System.Net;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public class NatMasterServer : INatPunchListener, INatMasterServer
    {
        private readonly NetworkConfig _networkConfig;
        private NetManager _net;
        private NatPunchModule _nat;

        private class HostInfo
        {
            public IPEndPoint Internal;
            public IPEndPoint External;
        }

        // token (room code) -> host info
        private readonly Dictionary<string, HostInfo> _hosts = new();

        public NatMasterServer(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
        }
        
        public void InitEntryPoint()
        {
            // var listener = new EventBasedNetListener();
            // _net = new NetManager(listener)
            // {
            //     IPv6Enabled = IPv6Mode.DualMode,
            //     NatPunchEnabled = true
            // };
            // var masterPort = _networkConfig.MasterPort;
            // _net.Start(masterPort);
            // _nat = _net.NatPunchModule;
            // _nat.Init(this);
            // LogService.LogTopic($"NAT master started on port {masterPort}", LogTopicType.NatMasterNetwork);
            // _ = Run();
        }

        private async Awaitable Run()
        {
            while (true)
            {
                _net.PollEvents();
                _nat.PollEvents();
                //await Awaitable.WaitForSecondsAsync(_networkConfig.DeltaTime);
            }
        }

        public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            LogService.LogTopic($" Intro request token={token} from {remoteEndPoint}, local={localEndPoint}", LogTopicType.NatMasterNetwork);

            if (!_hosts.TryGetValue(token, out var hostInfo))
            {
                // first caller with this token is host
                LogService.LogTopic($"Register host for token {token}", LogTopicType.NatMasterNetwork);
                _hosts[token] = new HostInfo
                {
                    Internal = localEndPoint,
                    External = remoteEndPoint
                };
                return;
            }

            // second caller is client: introduce host <-> client
            LogService.LogTopic($"[MASTER] Introduce host <-> client for token {token}", LogTopicType.NatMasterNetwork);

            _nat.NatIntroduce(
                hostInternal: hostInfo.Internal,
                hostExternal: hostInfo.External,
                clientInternal: localEndPoint,
                clientExternal: remoteEndPoint,
                additionalInfo: token
            );

            _hosts.Remove(token);
        }

        public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
        {
            // Usually not used on the master.
            LogService.LogTopic($"[MASTER] OnNatIntroductionSuccess: {targetEndPoint} type={type} token={token}", LogTopicType.NatMasterNetwork);
        }
    }
}
