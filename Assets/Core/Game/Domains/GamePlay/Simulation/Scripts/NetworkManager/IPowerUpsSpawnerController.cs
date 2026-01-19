namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface IPowerUpsSpawnerController
    {
        bool StepAndGetIsSpawnTimerEnded(float deltaTime);
    }
}