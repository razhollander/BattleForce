using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig
{
    public interface ISimulationGamePlayConfigService
    {
        void InitEntryPoint();
        void InitExitPoint();
        Configurations.SimulationGamePlayInnerConfig GamePlayConfig { get; }
        event Action<float> OnSpeedupSimulationChangedEvent;
        void OverrideGamePlayConfig(string configJson);
    }
}
