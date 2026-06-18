using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData
{
    [CreateAssetMenu(fileName = "DefaultMatchEnterDataConfig", menuName = "BF/Simulation/Default Match Enter Data Config")]
    public class DefaultMatchEnterDataConfig : ScriptableObject
    {
        public EnterMatchPlayerData[] Players;
        public SimulationMatchEnterData DefaultSimulationMatchEnterData;
    }

    [Serializable]
    public class SimulationMatchEnterData
    {
        public Dictionary<long, EnterMatchPlayerData[]> PlayersPerClient;
        public SimulationMatchEnterData(Dictionary<long, EnterMatchPlayerData[]> playersPerClient)
        {
            PlayersPerClient = playersPerClient;
        }
    }
}