namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MolesSpawner
{
    public interface IGoldenMoleSpawnedTrackerService
    {
        void ResetGoldenMoleSpawnCounter();
        bool ShouldSpawnGoldenMole();
        void RegisterMoleSpawned(bool isGolden);
    }
}
