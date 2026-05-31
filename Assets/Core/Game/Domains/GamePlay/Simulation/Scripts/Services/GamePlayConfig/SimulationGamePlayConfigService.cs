using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Helpers.JsonConverters;
using Newtonsoft.Json;

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
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TalentCooldownConfigConverter());
            _simulationGamePlayConfig = JsonConvert.DeserializeObject<SimulationGamePlayInnerConfig>(configJson, settings);
        }
    }
}