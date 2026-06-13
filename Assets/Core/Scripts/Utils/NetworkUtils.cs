using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Scripts.Utils
{
    public static class NetworkUtils
    {
        public static long GetDeviceUniqueId()
        {
            string rawId = SystemInfo.deviceUniqueIdentifier;
            byte[] stringBytes = Encoding.UTF8.GetBytes(rawId);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(stringBytes);
                return BitConverter.ToInt64(hashBytes, 0);
            }
        }
        
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
