using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Utils
{
    public static class TickUtils
    {
        public static int GetTickInTime(int currentTick, float timeInSeconds, float deltaTime)
        {
            return Mathf.RoundToInt(currentTick + timeInSeconds / deltaTime);
        }
        
        public static float GetSecondsLeftUntilTick(int currentTick, int goalTick, float deltaTime)
        {
            return (goalTick - currentTick) * deltaTime;
        }
    }
}
