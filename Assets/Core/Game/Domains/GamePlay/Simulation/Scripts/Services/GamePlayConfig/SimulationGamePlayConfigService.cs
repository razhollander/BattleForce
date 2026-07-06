using System;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Helpers.JsonConverters;
using Newtonsoft.Json;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig
{
    public class SimulationGamePlayConfigService : ISimulationGamePlayConfigService
    {
        private SimulationGamePlayInnerConfig _simulationGamePlayConfig;
        public SimulationGamePlayInnerConfig GamePlayConfig => _simulationGamePlayConfig;

        public event Action<float> OnSpeedupSimulationChangedEvent;
        private readonly SimulationGamePlayConfig _configAsset;

        public SimulationGamePlayConfigService(SimulationGamePlayConfig simulationGamePlayConfig)
        {
            _simulationGamePlayConfig = simulationGamePlayConfig.InnerConfig;
            _configAsset = simulationGamePlayConfig;
        }

        public void InitEntryPoint()
        {
            _configAsset.OnSpeedupSimulationChangedInEditorEvent += OnSpeedupSimulationChangedInEditor;
        }
        
        public void InitExitPoint()
        {
            _configAsset.OnSpeedupSimulationChangedInEditorEvent -= OnSpeedupSimulationChangedInEditor;
        }

        private void OnSpeedupSimulationChangedInEditor(float speedup)
        {
            OnSpeedupSimulationChangedEvent?.Invoke(speedup);
        }

        public void OverrideGamePlayConfig(string configJson)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TalentCooldownConfigConverter());
            var currentSimulationSpeed = _simulationGamePlayConfig.SpeedupSimulation;
            _simulationGamePlayConfig = JsonConvert.DeserializeObject<SimulationGamePlayInnerConfig>(configJson, settings);
            _simulationGamePlayConfig.SpeedupSimulation = currentSimulationSpeed;
        }
    }
}