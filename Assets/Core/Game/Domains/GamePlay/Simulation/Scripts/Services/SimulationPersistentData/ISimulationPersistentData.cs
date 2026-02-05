namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData
{
    public interface ISimulationPersistentData
    {
        bool ShouldSkipMatchMaking { get; }
        void InitEntryPoint();
    }
}