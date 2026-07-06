using System;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "SimulationGamePlayConfig", menuName = "BF/Simulation/GamePlay Config")]
    [System.Serializable]
    public class SimulationGamePlayConfig : ScriptableObject
    {
        public SimulationGamePlayInnerConfig InnerConfig;
        
        public event Action<float> OnSpeedupSimulationChangedInEditorEvent;

        private float _lastSpeedupSimulation;

        private void OnValidate()
        {
            float innerConfigSpeedupSimulation = InnerConfig.SpeedupSimulation;

            if (!Application.isPlaying)
            {
                _lastSpeedupSimulation = innerConfigSpeedupSimulation;
                return;
            }

            if (Math.Abs(_lastSpeedupSimulation - innerConfigSpeedupSimulation) < float.Epsilon)
            {
                return;
            }

            _lastSpeedupSimulation = innerConfigSpeedupSimulation;
            OnSpeedupSimulationChangedInEditorEvent?.Invoke(innerConfigSpeedupSimulation);
        }
    }
}
