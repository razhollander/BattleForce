using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "ScoreData", menuName = "Game/ScoreData", order = 1)]
public class ScoreScriptableObject : ScriptableObject
{
    public SerializableDictionary<int, int> PlayerIdToScore;
    //private <int, int> _playerIdToScore;
}
