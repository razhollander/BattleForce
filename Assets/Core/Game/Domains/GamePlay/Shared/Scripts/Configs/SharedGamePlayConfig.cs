using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using UnityEngine;

[CreateAssetMenu(fileName = "SharedGamePlayConfig", menuName = "BF/Shared/GamePlay Config")]
public class SharedGamePlayConfig : ScriptableObject
{
    public int MaxConcurrentTalentsForPlayer = 3;
    public int MaxTeamsAmount = 4;
    public EnvironmentConfig Environment;
    public MatchMakingEnvironmentLayoutConfig MatchMakingEnvironment;
    public PowerUpsSharedConfig PowerUps;
    public int MaxSavedPlaybacks = 10;
    public ushort NoTeamId = 5;
    public List<ushort> TeamIds = new List<ushort>() {1, 2, 3, 4};
    public ushort MinEntityId = 1; // 1 and not 0 because Box2D entites start from 1
}
