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
            // Determine the LAN IP without relying on DNS resolution of the machine hostname.
            // "Connecting" a UDP socket doesn't send any packets, but makes the OS pick the
            // outbound network interface, whose local endpoint is our real LAN IP.
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect("8.8.8.8", 65530);
                    if (socket.LocalEndPoint is IPEndPoint endPoint)
                    {
                        return endPoint.Address.ToString();
                    }
                }
            }
            catch (System.Exception e)
            {
                LogService.LogError("Error getting local IP via UDP socket: " + e.Message);
            }

            // Fallback: enumerate interface addresses directly (still no DNS).
            try
            {
                foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (System.Exception e)
            {
                LogService.LogError("Error getting local IP via host addresses: " + e.Message);
            }

            return "No IPv4 detected";
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
