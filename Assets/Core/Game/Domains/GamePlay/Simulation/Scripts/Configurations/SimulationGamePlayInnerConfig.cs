using System.Collections.Generic;
using ConditionalField;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;

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
        // Reused as the master switch and cadence for ALL Bonus Stages (Whac-A-Mole + GatePass). Kept under the
        // original names so the existing SimulationGamePlayConfig.asset values survive.
        public bool AreBonusStagesEnabled = true;
        [ConditionalField(nameof(AreBonusStagesEnabled), true)]
        public int BonusStageEveryXStages = 4;
        // Which bonus stage types the rotation may pick from. The rotation never repeats the same type twice in a
        // row, so with both entries present the bonus stages strictly alternate Whac-A-Mole <-> GatePass.
        [ConditionalField(nameof(AreBonusStagesEnabled), true)]
        public List<StageType> EnabledBonusStageTypes = new List<StageType> { StageType.WhacAMole, StageType.GatePass };
        // DeafultEnvironmentId names a DeathMatch layout, which a bonus stage cannot use, so each bonus stage type
        // needs its own pick for when ShouldChooseRandomStage is off.
        [ConditionalField(nameof(AreBonusStagesEnabled), true)]
        public int DefaultWhacAMoleEnvironmentId = 21;
        [ConditionalField(nameof(AreBonusStagesEnabled), true)]
        public int DefaultGatePassEnvironmentId = 22;
        public WhacAMoleConfig WhacAMole;
        public GatePassConfig GatePass;
        public float StageSizeMultiplier = 1.0f;
        public float SpeedupSimulation = 2f;
        public bool IsAutoShoot = false;
        public bool CanPlayersCollideWithEachOther = false;
        // Testing only: when true the server ignores real client input and drives every player with
        // fabricated "dumb player" input (see RandomPlayersInputService).
        public bool TestWithRandomPlayersInput = false;
    }
}