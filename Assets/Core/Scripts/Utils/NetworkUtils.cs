using System.Net;
using System.Net.Sockets;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

namespace Core.Scripts.Utils
{
    public static class NetworkUtils
    {
        public static string GetLocalIPAddress()
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
                LogService.LogError("Error getting local IP: " + e.Message);
                throw e;
            }
        }
    }
}
