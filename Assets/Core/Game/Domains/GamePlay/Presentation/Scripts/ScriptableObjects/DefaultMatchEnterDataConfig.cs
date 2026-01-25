using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultMatchEnterDataConfig", menuName = "BF/Simulation/Default Match Enter Data Config")]
public class DefaultMatchEnterDataConfig : ScriptableObject
{
    public MatchEnterData DefaultMatchEnterData;
}

[Serializable]
public class MatchEnterData
{
    public PlayerData[] Players;
    
    [Serializable]
    public class PlayerData
    {
        public int Id;
        public string Name;
        public int TeamId;
    }
}


