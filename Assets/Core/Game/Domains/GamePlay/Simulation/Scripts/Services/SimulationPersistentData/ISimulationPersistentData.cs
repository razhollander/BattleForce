namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData
{
    public interface ISimulationPersistentData
    {
        bool ShouldSkipMatchMaking { get; }
        long DeviceUniqueIdentifier { get; }
        void InitEntryPoint();
    }
}