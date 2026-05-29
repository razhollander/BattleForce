using ASoliman.Utils.EditableRefs;
using ConditionalField;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "SimulationGamePlayConfig", menuName = "BF/Simulation/GamePlay Config")]
    public class SimulationGamePlayConfig : ScriptableObject
    {
        [EditableRef] public PlayerSpaceshipConfig PlayerSpaceship;
        [EditableRef] public PlayerBulletConfig PlayerBullet;
        [EditableRef] public TalentsConfig Talents;
        [EditableRef] public LavaConfig Lava;
        [EditableRef] public PowerUpsConfig PowerUps;
        [EditableRef] public EnvironmentSpringsConfig EnvironmentSprings;
        [EditableRef] public PhysicsConfig Physics;
        public int DeafultEnvironmentId = 0;
        public bool ShouldChooseRandomStage = true;
        public bool ShouldChooseRandomTalentsForPlayer = true;
        [ConditionalField(nameof(ShouldChooseRandomTalentsForPlayer), true)]
        public int RandomTalentsForPlayersAmount = 1;
        public ushort StartMatchCountdownDuration = 5;
        public float ShootCooldownMultiplierWhenDead = 2;
        public float StageRestartDelaySeconds = 3f;
        public int GemsCollectedForTeamAlive = 1;
        public int MaxOverllapingFloors = 32;
        public int StartingBoltsPerTeam = 0;
        public int BoltsGainedPerHit = 50;
        public int BoltsGainedPerKill = 50;
        public float TeleportGateCooldownInSeconds = 0.5f;
        public float PreparationPhaseDuration = 5f;
        public bool ShouldAddTalentEveryXStages = false;
        [ConditionalField(nameof(ShouldAddTalentEveryXStages), true)]
        public int EveryXStages = 2;
        public float LockOnTargetHitDurationInSeconds = 2f;
    }
}