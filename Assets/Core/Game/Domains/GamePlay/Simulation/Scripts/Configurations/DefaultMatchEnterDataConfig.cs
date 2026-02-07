using System;
using Core.Game.Domains.GamePlay.Shared.Scripts;
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
        public EnterMatchPlayerData[] Players;

        public SimulationMatchEnterData(EnterMatchPlayerData[] players)
        {
            Players = players;
        }
    }
}