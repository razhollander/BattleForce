using System.Net;
using System.Net.Sockets;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public class MatchMakingUiController : IMatchMakingUiController
    {
        private readonly MatchMakingUiView _viewPrefab;
        private readonly IClientNetworkManager _clientNetworkManager;
        private MatchMakingUiView _view;

        public MatchMakingUiController(MatchMakingUiView viewPrefab, IClientNetworkManager clientNetworkManager)
        {
            _viewPrefab = viewPrefab;
            _clientNetworkManager = clientNetworkManager;
        }

        public void InitEntryPoint()
        {
            if (_viewPrefab == null)
            {
                Debug.LogWarning("MatchMakingUiView prefab is missing!");
                return;
            }

            _view = Object.Instantiate(_viewPrefab);

            if (_clientNetworkManager.IsHost)
            {
                string localIP = GetLocalIPAddress();
                _view.SetIpAddress("Host IP: " + localIP);
            }
            else
            {
                _view.SetIpAddress("");
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "No IPv4 detected";
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error getting local IP: " + e.Message);
                return "Error";
            }
        }
    }
}