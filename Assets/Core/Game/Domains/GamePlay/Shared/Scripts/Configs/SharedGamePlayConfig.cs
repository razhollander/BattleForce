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

    public bool ShouldMoveWithMouse; // keyboard players only - a gamepad player keeps steering with the stick

    public int MaxTeamsAmount = 4;
    [EditableRef] public EnvironmentConfig Environment; 
    public MatchMakingEnvironmentLayoutConfig MatchMakingEnvironment;
    [EditableRef] public PowerUpsSharedConfig PowerUps;
    [EditableRef] public EnvironmentTeleportConfig EnvironmentTeleport;
    public int MaxSavedPlaybacks = 40;
    public ushort NoTeamId = 5;
    public List<ushort> TeamIds = new List<ushort>() {1, 2, 3, 4};
    public ushort MinEntityId = 1; // 1 and not 0 because Box2D entites start from 1
    public UnityEngine.Vector2 FrigidBlockSize = new UnityEngine.Vector2(6f, 1.5f);
    public float GrapplingHookProjectileSize = 1f;
    public float GrapplingHookProjectileMaxDistance = 30f;
    public float FishingRodTipSize = 0.5f;
    public float SoulGhostSize = 1f;
    public float MagneticPullFieldRadius = 10f;
    public float LockOnTargetDurationInSeconds = 1.5f;
    public float LockOnTargetRetentionDurationInSeconds = 3f;
    public float HeadbuttMaxChargeDurationSeconds = 2f;
    public float MoleHoleShakeDurationSeconds = 0.6f; // the mole stays hidden and unhittable while its hole shakes
    public byte GoldenMoleDamagePerHit = 1; // lives a golden mole loses per hit, also the amount shown on its damage indicator
    public float MoleHideShakeDurationSeconds = 1f; // once its lifetime ends the mole shakes in place before hiding, and stays hittable while it does

    // GatePass ScoreGateObstacle geometry. Both the server (body) and the client (view) read these, so they live in the
    // Shared config. One gate = two square posts of ScoreGatePostSize with ScoreGateGapWidth between them. The gate's
    // mass/solver tuning is server-only and lives in GatePassConfig instead.
    public UnityEngine.Vector2 ScoreGatePostSize = new UnityEngine.Vector2(1.5f, 1.5f);
    public float ScoreGateGapWidth = 4f;
}
