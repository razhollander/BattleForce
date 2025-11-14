using System.IO;
using UnityEngine;

namespace CoreDomain.Scripts.Utils
{
    public static class IOUtils
    {
        public static void OpenFile(string filePath)
        {
            var absolutePath = Path.GetFullPath(filePath);
            if (File.Exists(absolutePath))
            {
                Application.OpenURL("file://" + absolutePath);
            }
        }
    }
}
