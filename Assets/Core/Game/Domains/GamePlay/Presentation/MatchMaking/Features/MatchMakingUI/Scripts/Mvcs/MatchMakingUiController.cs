using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public class MatchMakingUiController : IMatchMakingUiController
    {
        private readonly MatchMakingUiView _view;

        public MatchMakingUiController(MatchMakingUiView view)
        {
            _view = view;
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

        public void InitEntryPoint(string ipAddress, int port, bool isHost)
        {
            _view.Setup(isHost ? GetLocalIPAddress() : ipAddress, port.ToString());
        }
    }
}