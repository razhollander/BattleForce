using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationSpeedMultiplier
{
    public class SimulationSpeedMultiplierDataService : ISimulationSpeedMultiplierDataService
    {
        // Starts at 1 (normal speed). The speedup is only applied once a client connects.
        public float Multiplier { get; private set; } = 1f;

        public event Action OnMultiplierChangedEvent;

        public void SetMultiplier(float multiplier)
        {
            if (Math.Abs(Multiplier - multiplier) < float.Epsilon)
            {
                return;
            }

            Multiplier = multiplier;
            OnMultiplierChangedEvent?.Invoke();
        }
    }
}
