using System.Collections.Generic;
using ASoliman.Utils.EditableRefs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using UnityEngine;

[CreateAssetMenu(fileName = "SharedGamePlayConfig", menuName = "BF/Shared/GamePlay Config")]
public class SharedGamePlayConfig : ScriptableObject
{
    [EditableRef] public DefaultMatchEnterDataConfig DefaultMatchEnterDataConfig;
    public int MaxConcurrentTalentsForPlayer = 3;
    public int MaxTeamsAmount = 4;
    [EditableRef] public EnvironmentConfig Environment; 
    public MatchMakingEnvironmentLayoutConfig MatchMakingEnvironment;
    [EditableRef] public PowerUpsSharedConfig PowerUps;
    [EditableRef] public EnvironmentTeleportConfig EnvironmentTeleport;
    public int MaxSavedPlaybacks = 10;
    public ushort NoTeamId = 5;
    public List<ushort> TeamIds = new List<ushort>() {1, 2, 3, 4};
    public ushort MinEntityId = 1; // 1 and not 0 because Box2D entites start from 1
    public float GrapplingHookProjectileSize = 1f;
    public float GrapplingHookProjectileMaxDistance = 30f;
    public float MagneticPullFieldRadius = 5f;
    public float TargetMovementSpeed = 5f;
}
