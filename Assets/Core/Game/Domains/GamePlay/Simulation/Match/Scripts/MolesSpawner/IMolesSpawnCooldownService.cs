namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    public interface IMolesSpawnCooldownService
    {
        void InitEntryPoint();
        void ClearAllCooldowns();
        void RegisterMoleHoleToBeOnCooldown(ushort moleHoleId, int tick);
        bool IsMoleHoleOnCooldown(ushort moleHoleId, int tick);
    }
}
