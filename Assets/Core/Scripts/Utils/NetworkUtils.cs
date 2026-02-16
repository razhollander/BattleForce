using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
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
        
        public static async Task<string> GetPublicIpAddress()
        {
            using (var client = new HttpClient())
            {
                // We use a simple external service to echo back our IP
                return await client.GetStringAsync("https://api.ipify.org");
            }
        }
    }
}
