using System;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData
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