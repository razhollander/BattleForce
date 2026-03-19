using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Utils
{
    public static class TickUtils
    {
        public static int GetTickInTime(int currentTick, float timeInSeconds, float deltaTime)
        {
            return Mathf.RoundToInt(currentTick + timeInSeconds * deltaTime);
        }
    }
}
