using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Utils
{
    public static class TickUtils
    {
        public static int GetTickPassedAfterDuration(int currentTick, float durationInSeconds, float deltaTime)
        {
            return Mathf.RoundToInt(currentTick + durationInSeconds / deltaTime);
        }
        
        public static float GetSecondsLeftUntilTick(int currentTick, int goalTick, float deltaTime)
        {
            return Mathf.Max(0, goalTick - currentTick) * deltaTime;
        }
    }
}
