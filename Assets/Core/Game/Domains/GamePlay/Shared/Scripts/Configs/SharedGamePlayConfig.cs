using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

[CreateAssetMenu(fileName = "SharedGamePlayConfig", menuName = "BF/Shared/GamePlay Config")]
public class SharedGamePlayConfig : ScriptableObject
{
    public int MaxConcurrentTalentsForPlayer = 3;
    public EnvironmentConfig Environment;
    public MatchMakingEnvironmentLayoutConfig MatchMakingEnvironment;
    public PowerUpsSharedConfig PowerUps;
    public SerializableDictionary<int, Color> ColorPerTeamId;
}
