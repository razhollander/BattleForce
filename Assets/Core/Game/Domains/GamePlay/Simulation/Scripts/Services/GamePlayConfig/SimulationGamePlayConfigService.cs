using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig
{
    public class SimulationGamePlayConfigService : ISimulationGamePlayConfigService
    {
        private SimulationGamePlayInnerConfig _simulationGamePlayConfig;
        public SimulationGamePlayInnerConfig GamePlayConfig => _simulationGamePlayConfig;

        public SimulationGamePlayConfigService(SimulationGamePlayConfig simulationGamePlayConfig)
        {
            _simulationGamePlayConfig = simulationGamePlayConfig.InnerConfig;
        }
        
        public void OverrideGamePlayConfig(string configJson)
        {
            _simulationGamePlayConfig = JsonConvert.DeserializeObject<SimulationGamePlayInnerConfig>(configJson);
            int a = 0;
        }
    }
}