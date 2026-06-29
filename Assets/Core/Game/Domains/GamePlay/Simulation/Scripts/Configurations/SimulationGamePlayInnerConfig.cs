using ConditionalField;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class SimulationGamePlayInnerConfig
    {
        public PlayerSpaceshipConfig PlayerSpaceship;
        public PlayerBulletConfig PlayerBullet;
        public TalentsInnerConfig Talents;
        public LavaConfig Lava;
        public PowerUpsConfig PowerUps;
        public EnvironmentSpringsConfig EnvironmentSprings;
        public EnvironmentSpikesConfig EnvironmentSpikes;
        public int DeafultEnvironmentId = 6;
        public bool ShouldChooseRandomStage = true;
        public bool ShouldChooseRandomTalentsForPlayer = true;
        [ConditionalField(nameof(ShouldChooseRandomTalentsForPlayer), true)]
        public int RandomTalentsForPlayersAmount = 3;
        public ushort StartMatchCountdownDuration = 1;
        public float ShootCooldownMultiplierWhenDead = 2;
        public float StageRestartDelaySeconds = 3f;
        public int GemsCollectedForTeamAlive = 1;
        public int MaxOverllapingFloors = 32;
        public int StartingBoltsPerTeam = 0;
        public int BoltsGainedPerHit = 50;
        public int BoltsGainedPerKill = 50;
        public float TeleportGateCooldownInSeconds = 0.5f;
        public float PreparationPhaseDuration = 4f;
        public bool ShouldAddTalentEveryXStages = false;
        [ConditionalField(nameof(ShouldAddTalentEveryXStages), true)]
        public int EveryXStages = 2;
        public float StageSizeMultiplier = 1.0f;
        public float SpeedupSimulation = 2f;
    }
}