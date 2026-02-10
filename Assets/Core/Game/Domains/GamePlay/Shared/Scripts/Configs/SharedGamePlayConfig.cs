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
}
