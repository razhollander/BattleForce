using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.SimulationSpeedMultiplier
{
    public interface ISimulationSpeedMultiplierDataService
    {
        float Multiplier { get; }
        event Action OnMultiplierChangedEvent;
        void SetMultiplier(float multiplier);
    }
}
