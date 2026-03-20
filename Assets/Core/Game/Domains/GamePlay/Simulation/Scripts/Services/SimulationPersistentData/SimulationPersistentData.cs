using Core.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationPersistentData
{
    /// <summary>
    /// We can't use player prefs in a custom thread, so we cache them here 
    /// </summary>
    public class SimulationPersistentData : ISimulationPersistentData
    {
        public bool ShouldSkipMatchMaking { get; private set; }

        public SimulationPersistentData()
        {
        }

        public void InitEntryPoint()
        {
            ShouldSkipMatchMaking = PlayerPrefsSettings.ShouldSkipMatchMaking;
        }
    }
}