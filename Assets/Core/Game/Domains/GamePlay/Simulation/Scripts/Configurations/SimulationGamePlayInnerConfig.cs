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
        public bool IsWhacAMoleModeEnabled = true;
        [ConditionalField(nameof(IsWhacAMoleModeEnabled), true)]
        public int WhacAMoleEveryXStages = 3;
        // DeafultEnvironmentId names a DeathMatch layout, which a WhacAMole stage cannot use, so that
        // stage type needs its own pick for when ShouldChooseRandomStage is off.
        [ConditionalField(nameof(IsWhacAMoleModeEnabled), true)]
        public int DefaultWhacAMoleEnvironmentId = 21;
        public WhacAMoleConfig WhacAMole;
        public float StageSizeMultiplier = 1.0f;
        public float SpeedupSimulation = 2f;
        public bool IsAutoShoot = false;
        public bool CanPlayersCollideWithEachOther = false;
        // Testing only: when true the server ignores real client input and drives every player with
        // fabricated "dumb player" input (see RandomPlayersInputService).
        public bool TestWithRandomPlayersInput = false;
    }
}