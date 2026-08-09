using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        private readonly MatchSimulationStateS2C _simulationState;
        public MatchSimulationStateS2C SimulationState => _simulationState;
        private ushort _lastBulletCreatedId = 0;
        private ushort _lastPowerUpBallCreatedId = 0;
        private ushort _lastSwapFieldCreatedId = 0;
        private ushort _lastKOProjectileCreatedId = 0;
        private ushort _lastGrapplingHookProjectileCreatedId = 0;
        private ushort _lastFishingRodProjectileCreatedId = 0;
        private ushort _lastSoulGhostCreatedId = 0;
        private ushort _lastFrigidBlockCreatedId = 0;
        private ushort _lastChickenEggCreatedId = 0;
        private ushort _lastGalacticForceFieldCreatedId = 0;
        private ushort _lastMoleCreatedId = 0;

        private readonly Dictionary<StageType, List<int>> _didntPlayYetStageIndexesPerStageType = new Dictionary<StageType, List<int>>
        {
            { StageType.DeathMatch, new List<int>() },
            { StageType.WhacAMole, new List<int>() },
            { StageType.GatePass, new List<int>() },
        };

        public MatchEnvironmentDataService EnvironmentData { get; private set; }
        public HashSet<ushort> TeamIds { get; private set; }

        public MatchDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            EnvironmentData = new MatchEnvironmentDataService(networkConfig);
            var maxCap = networkConfig.MaxCap;

            _simulationState = new MatchSimulationStateS2C(
                maxCap.ConcurrentPlayers,
                maxCap.ConcurrentBullets,
                sharedGamePlayConfig.MaxConcurrentTalentsForPlayer,
                maxCap.ConcurrentTalentCards,
                maxCap.ConcurrentPowerUpBalls,
                sharedGamePlayConfig.MaxTeamsAmount,
                maxCap.ConcurrentChickenEggs,
                maxCap.ConcurrentGalacticForceFields,
                maxCap.ConcurrentFrigidBlocks,
                maxCap.ConcurrentMoles,
                maxCap.ConcurrentScoreGates);

            TeamIds = new HashSet<ushort>(sharedGamePlayConfig.MaxTeamsAmount);
            _simulationState.GemsPerTeamId = new Dictionary<ushort, int>(sharedGamePlayConfig.MaxTeamsAmount);
            _simulationState.StageType = StageType.DeathMatch;
        }
        
        public PlayerStateS2C AddPlayer(ushort playerId, ushort teamId, string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius, ushort health,
            float shootCooldown)
        {
            var newPlayer = _simulationState.Players.AddAndGet();
            newPlayer.Id = playerId;
            newPlayer.Name = playerName;
            newPlayer.TeamId = teamId;
            newPlayer.Spaceship.Health.CurrentHealth = health;
            newPlayer.Spaceship.Health.MaxHealth = health;
            newPlayer.Spaceship.Transform.Position = position;
            newPlayer.Spaceship.Transform.Direction = direction;
            newPlayer.Spaceship.Transform.Velocity = velocity;
            newPlayer.Spaceship.Transform.Radius = radius;
            newPlayer.Spaceship.Shoot.CooldownSecondsLeft = shootCooldown;
            newPlayer.Spaceship.Shoot.MaxCooldown = shootCooldown;
            TeamIds.Add(teamId);
            _simulationState.GemsPerTeamId.TryAdd(teamId, 0);
            _simulationState.BoltsPerTeam.TryAdd(teamId, 0);
            _simulationState.MolesHitPerTeamId.TryAdd(teamId, 0);
            return newPlayer;
        }

        public List<int> GetDidntPlayYetStageIndexes(StageType stageType)
        {
            return _didntPlayYetStageIndexesPerStageType[stageType];
        }

        public PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius, int createdOnTick)
        {
            ref var playerBullet = ref _simulationState.Bullets.AddAndGet();
            var bulletId =(ushort) (++_lastBulletCreatedId % ushort.MaxValue);
            playerBullet.CreatedOnTick = createdOnTick;
            playerBullet.Id = bulletId;
            playerBullet.BelongToPlayerId = belongToPlayerId;
            playerBullet.Position = position;
            playerBullet.Direction = direction;
            playerBullet.Radius = radius;
            playerBullet.Velocity = direction * moveSpeed;
            return playerBullet;
        }

        public TalentCardS2C AddTalentCard(ushort talentCardId, Vector2 position, TalentType talentType, ushort Health)
        {
            ref var newCard = ref _simulationState.TalentCards.AddAndGet();
            newCard.Id = talentCardId;
            newCard.Position = position;
            newCard.TalentType = talentType;
            newCard.Health = Health;
            return newCard;
        }

        public PowerUpBallS2C AddPowerUpBall(Vector2 position, Vector2 velocity, PowerUpType powerUpType)
        {
            ref var powerUpBall = ref _simulationState.PowerUpBalls.AddAndGet();
            var powerUpBallId =(ushort) (++_lastPowerUpBallCreatedId % byte.MaxValue);
            powerUpBall.Id = powerUpBallId;
            powerUpBall.Position = position;
            powerUpBall.Velocity = velocity;
            return powerUpBall;
        }

        public MoleStateS2C AddMole(Vector2 position, int emergeOnTick, int disappearOnTick, bool isGolden, byte lives)
        {
            ref var mole = ref _simulationState.Moles.AddAndGet();
            var moleId = (ushort)(++_lastMoleCreatedId % byte.MaxValue);
            mole.Id = moleId;
            mole.Position = position;
            mole.EmergeOnTick = emergeOnTick;
            mole.IsEmerged = false;
            mole.DisappearOnTick = disappearOnTick;
            mole.HideOnTick = 0;
            mole.IsGolden = isGolden;
            mole.RemainingLives = lives;
            mole.MaxLives = lives;
            return mole;
        }

        // A score gate uses its authored layout id (not an auto-incremented one), so the same gate keeps its id across rejoins.
        public ScoreGateStateS2C AddScoreGate(ushort id, Vector2 position, float rotationDegrees)
        {
            ref var scoreGate = ref _simulationState.ScoreGates.AddAndGet();
            scoreGate.Id = id;
            scoreGate.Position = position;
            scoreGate.Rotation = rotationDegrees.ToRadians().AngleToVector();
            scoreGate.LastScoredTeamId = 0;
            return scoreGate;
        }

        public TalentSwapFieldS2C AddSwapField(ushort casterPlayerId, int tick, int fieldEndTick)
        {
            ref var swapField = ref _simulationState.SwapFields.AddAndGet();
            var swapFieldId =(ushort) (++_lastSwapFieldCreatedId % byte.MaxValue);
            swapField.Id = swapFieldId;
            swapField.PlayerCasterId = casterPlayerId;
            swapField.CreatedOnTick = tick;
            swapField.EndTick = fieldEndTick;
            return swapField;
        }

        public TalentKOProjectileS2C AddKOProjectile(int tick, ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 velocity, float size)
        {
            ref var koProjectile = ref _simulationState.KOProjectiles.AddAndGet();
            var koProjectileId = (ushort)(++_lastKOProjectileCreatedId % byte.MaxValue);
            koProjectile.CreatedOnTick = tick;
            koProjectile.Id = koProjectileId;
            koProjectile.PlayerCasterId = casterPlayerId;
            koProjectile.Position = position;
            koProjectile.Rotation = rotation;
            koProjectile.Velocity = velocity;
            koProjectile.Size = size;
            return koProjectile;
        }

        public TalentGrapplingHookProjectileStateS2C AddGrapplingHookProjectile(ushort casterPlayerId, Vector2 position, Vector2 velocity)
        {
            ref var grapplingHookProjectile = ref _simulationState.GrapplingHookProjectiles.AddAndGet();
            var projectileId = (ushort)(++_lastGrapplingHookProjectileCreatedId % byte.MaxValue); 
            grapplingHookProjectile.Id = projectileId;
            grapplingHookProjectile.PlayerCasterId = casterPlayerId;
            grapplingHookProjectile.StartPosition = position;
            grapplingHookProjectile.Position = position;
            grapplingHookProjectile.Velocity = velocity;
            grapplingHookProjectile.HitData = default;
            return grapplingHookProjectile;
        }

        public TalentFishingRodProjectileStateS2C AddFishingRodProjectile(ushort casterPlayerId, Vector2 position, Vector2 velocity)
        {
            ref var fishingRodProjectile = ref _simulationState.FishingRodProjectiles.AddAndGet();
            var projectileId = (ushort)(++_lastFishingRodProjectileCreatedId % byte.MaxValue);
            fishingRodProjectile.Id = projectileId;
            fishingRodProjectile.PlayerCasterId = casterPlayerId;
            fishingRodProjectile.Position = position;
            fishingRodProjectile.Velocity = velocity;
            fishingRodProjectile.Phase = FishingRodTipPhase.FlyingForward;
            fishingRodProjectile.CaughtEnemyId = 0;
            fishingRodProjectile.CaughtEnemyType = FishingRodCaughtEnemyType.None;
            fishingRodProjectile.EnemyCaughtArrowDirection = Vector2.Zero;
            return fishingRodProjectile;
        }

        public TalentSoulGhostStateS2C AddSoulGhost(ushort casterPlayerId, Vector2 position, Vector2 direction, Vector2 velocity)
        {
            ref var soulGhost = ref _simulationState.SoulGhosts.AddAndGet();
            var ghostId = (ushort)(++_lastSoulGhostCreatedId % byte.MaxValue);
            soulGhost.Id = ghostId;
            soulGhost.PlayerCasterId = casterPlayerId;
            soulGhost.Position = position;
            soulGhost.Direction = direction;
            soulGhost.Velocity = velocity;
            return soulGhost;
        }

        public TalentFrigidBlockStateS2C AddFrigidBlock(ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 velocity)
        {
            ref var frigidBlock = ref _simulationState.FrigidBlocks.AddAndGet();
            var blockId = (ushort)(++_lastFrigidBlockCreatedId % byte.MaxValue);
            frigidBlock.Id = blockId;
            frigidBlock.PlayerCasterId = casterPlayerId;
            frigidBlock.Position = position;
            frigidBlock.Rotation = rotation;
            frigidBlock.Velocity = velocity;
            frigidBlock.AngularVelocity = 0f;
            return frigidBlock;
        }

        public TalentChickenEggStateS2C AddChickenEgg(ushort casterPlayerId, Vector2 position)
        {
            ref var egg = ref SimulationState.ChickenEggs.AddAndGet();
            var eggId = (ushort)(++_lastChickenEggCreatedId % ushort.MaxValue);
            egg.Id = eggId;
            egg.PlayerCasterId = casterPlayerId;
            egg.Position = position;

            return egg;
        }

        public GalacticForceFieldS2C AddGalacticForceField(ushort casterTeamId, int endTick)
        {
            ref var field = ref SimulationState.GalacticForceFields.AddAndGet();
            var fieldId = (ushort)(++_lastGalacticForceFieldCreatedId % byte.MaxValue);
            field.Id = fieldId;
            field.CasterTeamId = casterTeamId;
            field.EndTick = endTick;
            return field;
        }
    }
}