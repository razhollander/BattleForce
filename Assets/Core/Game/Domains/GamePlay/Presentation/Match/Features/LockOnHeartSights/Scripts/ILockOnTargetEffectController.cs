namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.LockOnHeartSights.Scripts
{
    public interface ILockOnTargetEffectController
    {
        void InitEntryPoint();
        void InitExitPoint();
        void UpdateEffects();
        void UpdateTargetsPositionOnPlayer(UnityEngine.Vector3 playerHeartPosition);
    }
}
