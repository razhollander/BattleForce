namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate
{
    public interface IEnvironmentTeleportGateControllers
    {
        void InitEntryPoint();
        EnvironmentTeleportGatePairController CreateGatePair(ushort pairId);
        void DestroyAll();
        void PlayTeleportAnimation(ushort pairId);
        void UpdateTeleportGateTransform(ushort pairId);
    }
}