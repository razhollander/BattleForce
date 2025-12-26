using Core.Scripts.Extensions;
using CoreDomain.Scripts.Extensions;
using TMPro;
using UnityEngine;
using Utils;

public class ScorePanel : MonoBehaviour
{
    [SerializeField] private SerializableDictionary<int, TextMeshProUGUI> _playerIdToText;

    public void UpdateAllTexts(SerializableDictionary<int, int> scoreDataPlayerIdToScore)
    {
        _playerIdToText.ForEach(x => x.Value.text = scoreDataPlayerIdToScore[x.Key].ToString());
    }

    public void SetPlayerColor(int id, Color color)
    {
        _playerIdToText[id].color = color;
    }
}
