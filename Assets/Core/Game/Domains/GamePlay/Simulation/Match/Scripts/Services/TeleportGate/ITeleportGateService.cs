namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate
{
    public interface ITeleportGateService
    {
        void RegisterTeleport(ushort playerId, int currentTick);
        bool IsTeleportOnCooldown(ushort playerId, int currentTick);
        void ClearData();
    }
}
