using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ScoreService : MonoBehaviour
{
    [SerializeField] private List<ScorePanel> _scorePanels;
    [SerializeField] private ScoreScriptableObject _scoreData;
    
    private void Awake()
    {
        if (_scoreData.PlayerIdToScore == null || _scoreData.PlayerIdToScore.Count == 0)
        {
            SetupScoreData();
        }
        
        UpdateScoreText();
        
        Application.targetFrameRate = 60;
    }

    private void SetupScoreData()
    {
        _scoreData.PlayerIdToScore = new SerializableDictionary<int, int>();
        var players = GameObject.FindObjectsOfType<PlayerCircle>();
        
        foreach (var player in players)
        {
            _scoreData.PlayerIdToScore.Add(player.ID, 0);
        }
    }

    public void IncrementAllPlayersAliveScore()
    {
        var players = FindObjectsOfType<PlayerCircle>();
        
        foreach (var player in players)
        {
            if (!player.IsDead)
            {
                AddScoreToPlayer(player.ID);
            }
        }

        UpdateScoreText();
    }
    
    private void AddScoreToPlayer(int playerId)
    {
        _scoreData.PlayerIdToScore[playerId] += 1;
    }

    private void UpdateScoreText()
    {
        _scorePanels.ForEach(x=>x.UpdateAllTexts(_scoreData.PlayerIdToScore));
    }

    private void OnApplicationQuit()
    {
        _scoreData.PlayerIdToScore.Clear();
    }
    // private ScoreService()
    // {
    //     _playerIdToScore = new Dictionary<int, int>();
    //     var players = GameObject.FindObjectsOfType<PlayerCircle>();
    //
    //     foreach (var player in players)
    //     {
    //         _playerIdToScore.Add(player.ID, 0);
    //     }
    //
    //     UpdateScoreText();
    // }
    public void SetColorForPlayer(int id, Color color)
    {
        foreach (var scorePanel in _scorePanels)
        {
            scorePanel.SetPlayerColor(id, color);
        }
    }
}
