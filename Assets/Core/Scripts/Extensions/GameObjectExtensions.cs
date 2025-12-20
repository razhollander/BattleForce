using UnityEngine;

namespace Core.Scripts.Extensions
{
    public static class GameObjectExtensions
    {
        public static void TrySetActive(this GameObject go, bool isActive)
        {
            if (go.activeSelf == isActive)
            {
                return;
            }
            
            go.SetActive(isActive);
        }
    }
}
