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
        public struct PlayerData
        {
            public ushort Id;
            public string Name;
            public ushort TeamId;
        }

        public SimulationMatchEnterData(PlayerData[] players)
        {
            Players = players;
        }
    }
}