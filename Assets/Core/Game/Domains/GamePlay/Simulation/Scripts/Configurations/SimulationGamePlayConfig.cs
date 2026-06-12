using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "SimulationGamePlayConfig", menuName = "BF/Simulation/GamePlay Config")]
    [System.Serializable]
    public class SimulationGamePlayConfig : ScriptableObject
    {
        public SimulationGamePlayInnerConfig InnerConfig;
    }
}