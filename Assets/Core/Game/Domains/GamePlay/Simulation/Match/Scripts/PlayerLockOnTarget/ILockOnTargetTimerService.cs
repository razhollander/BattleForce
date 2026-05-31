using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public interface ILockOnTargetTimerService
    {
        void StepTimers(float deltaTime);
        List<(ushort CasterId, ushort TargetId)> GetPlayersToDamage();
        void ResetTimer(ushort casterId, ushort targetId);
        void ResetAllTimers();
    }
}
