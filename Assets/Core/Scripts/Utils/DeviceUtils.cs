using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Core.Scripts.Utils
{
    public static class DeviceUtils
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
    }
}
