using System;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "DefaultMatchEnterDataConfig", menuName = "BF/Simulation/Default Match Enter Data Config")]
    public class DefaultMatchEnterDataConfig : ScriptableObject
    {
        public SimulationMatchEnterData DefaultSimulationMatchEnterData;
    }

    [Serializable]
    public class SimulationMatchEnterData
    {
        public PlayerData[] Players;
    
        [Serializable]
        public class PlayerData
        {
            public ushort Id;
            public string Name;
            public ushort TeamId;
        }
    }
}