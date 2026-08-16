using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    /// <summary>
    /// This class holds all the data needed to recreate a state snapshot of the game. nothing else.
    /// </summary>
    public class MatchSimulationStateS2C
    {
        public FixedClassUnorderedList<PlayerStateS2C> Players;
        public FixedOrderedList<PlayerBulletS2C> Bullets;
        public FixedUnorderedList<TalentCardS2C> TalentCards;
        public FixedUnorderedList<PowerUpBallS2C> PowerUpBalls;
        public FixedUnorderedList<TalentSwapFieldS2C> SwapFields;
        public FixedUnorderedList<TalentKOProjectileS2C> KOProjectiles;
        public FixedUnorderedList<TalentGrapplingHookProjectileStateS2C> GrapplingHookProjectiles;
        public FixedUnorderedList<TalentFishingRodProjectileStateS2C> FishingRodProjectiles;
        public FixedUnorderedList<TalentSoulGhostStateS2C> SoulGhosts;
        public FixedUnorderedList<TalentFrigidBlockStateS2C> FrigidBlocks;
        public FixedUnorderedList<TalentChickenEggStateS2C> ChickenEggs;
        public FixedUnorderedList<GalacticForceFieldS2C> GalacticForceFields;
        public FixedUnorderedList<MoleStateS2C> Moles;
        public FixedUnorderedList<ScoreGateStateS2C> ScoreGates;
        public FixedUnorderedList<EnvironmentGateTrapStateS2C> GateTraps;
        public Dictionary<ushort, int> GemsPerTeamId;
        public Dictionary<ushort, int> BoltsPerTeam;
        public Dictionary<ushort, int> MolesHitPerTeamId;
        public FixedOrderedList<ushort> FieldBarriersOrderedByTeamId;
        public int EnvironmentLayoutId;
        public StageType StageType;
        public int WhacAMoleEndTick;
        public int PreperationPhaseStartedOnTick;
        public int PreperationPhaseEndedOnTick;
        public bool IsInPreparationPhase;
        public bool IsInShowoffWinners;
        public ushort CurrentStageWinnerTeamId;
        public float MapSizeMultiplier;
        
        public MatchSimulationStateS2C(int maxPlayers, int maxBullets, int maxTalentsPerPlayer, int maxTalentCards, int maxPowerUpBalls, int maxTeams, int maxChickenEggs, int maxGalacticForceFields, int maxFrigidBlocks,
            int maxMoles, int maxScoreGates, int maxGateTraps)
        {
            // The lock on capacity must stay in sync with MaxCap.ConcurrentLockOnTargets, a caster can lock on every enemy, power up ball and mole at once
            Players = new FixedClassUnorderedList<PlayerStateS2C>(maxPlayers, ()=>new PlayerStateS2C(maxTalentsPerPlayer, maxPlayers - 1 + maxPowerUpBalls + maxMoles));
            Bullets = new FixedOrderedList<PlayerBulletS2C>(maxBullets);
            TalentCards = new FixedUnorderedList<TalentCardS2C>(maxTalentCards);
            PowerUpBalls = new FixedUnorderedList<PowerUpBallS2C>(maxPowerUpBalls);
            SwapFields = new FixedUnorderedList<TalentSwapFieldS2C>(maxPlayers);
            KOProjectiles = new FixedUnorderedList<TalentKOProjectileS2C>(maxPlayers);
            GrapplingHookProjectiles = new FixedUnorderedList<TalentGrapplingHookProjectileStateS2C>(maxPlayers);
            FishingRodProjectiles = new FixedUnorderedList<TalentFishingRodProjectileStateS2C>(maxPlayers);
            SoulGhosts = new FixedUnorderedList<TalentSoulGhostStateS2C>(maxPlayers);
            FrigidBlocks = new FixedUnorderedList<TalentFrigidBlockStateS2C>(maxFrigidBlocks);
            ChickenEggs = new FixedUnorderedList<TalentChickenEggStateS2C>(maxChickenEggs);
            GalacticForceFields = new FixedUnorderedList<GalacticForceFieldS2C>(maxGalacticForceFields);
            Moles = new FixedUnorderedList<MoleStateS2C>(maxMoles);
            ScoreGates = new FixedUnorderedList<ScoreGateStateS2C>(maxScoreGates);
            GateTraps = new FixedUnorderedList<EnvironmentGateTrapStateS2C>(maxGateTraps);
            GemsPerTeamId = new Dictionary<ushort, int>(maxTeams);
            BoltsPerTeam = new Dictionary<ushort, int>(maxTeams);
            MolesHitPerTeamId = new Dictionary<ushort, int>(maxTeams);
            FieldBarriersOrderedByTeamId = new FixedOrderedList<ushort>(maxTeams);
        }

        public void Serialize(NetDataWriter writer)
        {
            var amountOfTeams = (byte)GemsPerTeamId.Count;
            writer.Put(amountOfTeams);

            var playerCount = Players.Count;
            writer.Put((byte)playerCount);
            foreach (var player in Players.AsSpan())
            {
                player.Serialize(writer);
            }
        
            var bulletsCount = Bullets.Count;
            writer.Put((byte)bulletsCount);
            foreach (var bullet in Bullets.AsSpan())
            {
                bullet.Serialize(writer);
            }
            
            var talentCardsCount = TalentCards.Count;
            writer.Put((byte)talentCardsCount);
            foreach (var talentCard in TalentCards.AsSpan())
            {
                talentCard.Serialize(writer);
            }

            var powerUpsCount = PowerUpBalls.Count;
            writer.Put((byte)powerUpsCount);
            foreach (var powerUp in PowerUpBalls.AsSpan())
            {
                powerUp.Serialize(writer);
            }

            var molesCount = Moles.Count;
            writer.Put((byte)molesCount);
            foreach (var mole in Moles.AsSpan())
            {
                mole.Serialize(writer);
            }

            var scoreGatesCount = ScoreGates.Count;
            writer.Put((byte)scoreGatesCount);
            foreach (var scoreGate in ScoreGates.AsSpan())
            {
                scoreGate.Serialize(writer);
            }
            
            var gateTrapsCount = GateTraps.Count;
            writer.Put((byte)gateTrapsCount);
            foreach (var gateTrap in GateTraps.AsSpan())
            {
                gateTrap.Serialize(writer);
            }

            foreach (var kvp in GemsPerTeamId)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }

            foreach (var kvp in BoltsPerTeam)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }

            foreach (var kvp in MolesHitPerTeamId)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }

            var swapFieldsCount = SwapFields.Count;
            writer.Put((byte)swapFieldsCount);
            foreach (var swapField in SwapFields.AsSpan())
            {
                swapField.Serialize(writer);
            }

            var koProjectilesCount = KOProjectiles.Count;
            writer.Put((byte)koProjectilesCount);
            foreach (var koProjectile in KOProjectiles.AsSpan())
            {
                koProjectile.Serialize(writer);
            }

            var grapplingHookProjectilesCount = GrapplingHookProjectiles.Count;
            writer.Put((byte)grapplingHookProjectilesCount);
            foreach (var grapplingHookProjectile in GrapplingHookProjectiles.AsSpan())
            {
                grapplingHookProjectile.Serialize(writer);
            }

            var fishingRodProjectilesCount = FishingRodProjectiles.Count;
            writer.Put((byte)fishingRodProjectilesCount);
            foreach (var fishingRodProjectile in FishingRodProjectiles.AsSpan())
            {
                fishingRodProjectile.Serialize(writer);
            }

            var soulGhostsCount = SoulGhosts.Count;
            writer.Put((byte)soulGhostsCount);
            foreach (var soulGhost in SoulGhosts.AsSpan())
            {
                soulGhost.Serialize(writer);
            }

            var frigidBlocksCount = FrigidBlocks.Count;
            writer.Put((byte)frigidBlocksCount);
            foreach (var frigidBlock in FrigidBlocks.AsSpan())
            {
                frigidBlock.Serialize(writer);
            }

            var chickenEggsCount = ChickenEggs.Count;
            writer.Put((byte)chickenEggsCount);
            foreach (var chickenEgg in ChickenEggs.AsSpan())
            {
                chickenEgg.Serialize(writer);
            }

            var galacticForceFieldsCount = GalacticForceFields.Count;
            writer.Put((byte)galacticForceFieldsCount);
            foreach (var field in GalacticForceFields.AsSpan())
            {
                field.Serialize(writer);
            }

            foreach (var teamId in FieldBarriersOrderedByTeamId.AsSpan())
            {
                writer.Put((byte)teamId);
            }

            writer.Put((byte)EnvironmentLayoutId);
            writer.Put((byte)StageType);
            writer.Put(WhacAMoleEndTick);
            writer.Put(PreperationPhaseStartedOnTick);
            writer.Put(PreperationPhaseEndedOnTick);
            writer.Put(IsInPreparationPhase);
            writer.Put(IsInShowoffWinners);
            writer.Put((byte)CurrentStageWinnerTeamId);
            writer.PutFloat16(MapSizeMultiplier);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            var amountOfTeams = reader.GetByte();
            
            var playersCount = reader.GetByte();
            Players.Clear();
            for (var i = 0; i < playersCount; i++)
            {
                var player = Players.AddAndGet();
                player.Deserialize(reader);;
            }
          
            var bulletsCount = reader.GetByte();
            Bullets.Clear();
            for (var i = 0; i < bulletsCount; i++)
            {
                ref var bullet = ref Bullets.AddAndGet();
                bullet.Deserialize(reader);
            }

            var talentCardsCount = reader.GetByte();
            TalentCards.Clear();
            for (var i = 0; i < talentCardsCount; i++)
            {
                ref var talentCard = ref TalentCards.AddAndGet();
                talentCard.Deserialize(reader);
            }

            var powerUpsCount = reader.GetByte();
            PowerUpBalls.Clear();
            for (var i = 0; i < powerUpsCount; i++)
            {
                ref var powerUp = ref PowerUpBalls.AddAndGet();
                powerUp.Deserialize(reader);
            }
            
            var molesCount = reader.GetByte();
            Moles.Clear();
            for (var i = 0; i < molesCount; i++)
            {
                ref var mole = ref Moles.AddAndGet();
                mole.Deserialize(reader);
            }

            var scoreGatesCount = reader.GetByte();
            ScoreGates.Clear();
            for (var i = 0; i < scoreGatesCount; i++)
            {
                ref var scoreGate = ref ScoreGates.AddAndGet();
                scoreGate.Deserialize(reader);
            }

            var gateTrapsCount = reader.GetByte();
            GateTraps.Clear();
            for (var i = 0; i < gateTrapsCount; i++)
            {
                ref var gateTrap = ref GateTraps.AddAndGet();
                gateTrap.Deserialize(reader);
            }

            GemsPerTeamId.Clear();
            for (int i = 0; i < amountOfTeams; i++)
            {
                var teamId = reader.GetUShort();
                var jems = reader.GetInt();
                GemsPerTeamId.Add(teamId, jems);
            }

            BoltsPerTeam.Clear();
            for (int i = 0; i < amountOfTeams; i++)
            {
                var teamId = reader.GetUShort();
                var bolts = reader.GetInt();
                BoltsPerTeam.Add(teamId, bolts);
            }

            MolesHitPerTeamId.Clear();
            for (int i = 0; i < amountOfTeams; i++)
            {
                var teamId = reader.GetUShort();
                var molesHit = reader.GetInt();
                MolesHitPerTeamId.Add(teamId, molesHit);
            }

            var swapFieldsCount = reader.GetByte();
            SwapFields.Clear();
            for (var i = 0; i < swapFieldsCount; i++)
            {
                ref var swapField = ref SwapFields.AddAndGet();
                swapField.Deserialize(reader);
            }
            
            var koProjectilesCount = reader.GetByte();
            KOProjectiles.Clear();
            for (var i = 0; i < koProjectilesCount; i++)
            {
                ref var koProjectile = ref KOProjectiles.AddAndGet();
                koProjectile.Deserialize(reader);
            }

            var grapplingHookProjectilesCount = reader.GetByte();
            GrapplingHookProjectiles.Clear();
            for (var i = 0; i < grapplingHookProjectilesCount; i++)
            {
                ref var grapplingHookProjectile = ref GrapplingHookProjectiles.AddAndGet();
                grapplingHookProjectile.Deserialize(reader);
            }

            var fishingRodProjectilesCount = reader.GetByte();
            FishingRodProjectiles.Clear();
            for (var i = 0; i < fishingRodProjectilesCount; i++)
            {
                ref var fishingRodProjectile = ref FishingRodProjectiles.AddAndGet();
                fishingRodProjectile.Deserialize(reader);
            }

            var soulGhostsCount = reader.GetByte();
            SoulGhosts.Clear();
            for (var i = 0; i < soulGhostsCount; i++)
            {
                ref var soulGhost = ref SoulGhosts.AddAndGet();
                soulGhost.Deserialize(reader);
            }

            var frigidBlocksCount = reader.GetByte();
            FrigidBlocks.Clear();
            for (var i = 0; i < frigidBlocksCount; i++)
            {
                ref var frigidBlock = ref FrigidBlocks.AddAndGet();
                frigidBlock.Deserialize(reader);
            }

            var chickenEggsCount = reader.GetByte();
            ChickenEggs.Clear();
            for (var i = 0; i < chickenEggsCount; i++)
            {
                ref var chickenEgg = ref ChickenEggs.AddAndGet();
                chickenEgg.Deserialize(reader);
            }

            var galacticForceFieldsCount = reader.GetByte();
            GalacticForceFields.Clear();
            for (var i = 0; i < galacticForceFieldsCount; i++)
            {
                ref var field = ref GalacticForceFields.AddAndGet();
                field.Deserialize(reader);
            }

            FieldBarriersOrderedByTeamId.Clear();
            for (var i = 0; i < amountOfTeams; i++)
            {
                ref var teamId = ref FieldBarriersOrderedByTeamId.AddAndGet();
                teamId = reader.GetByte();
            }

            EnvironmentLayoutId = reader.GetByte();
            StageType = (StageType)reader.GetByte();
            WhacAMoleEndTick = reader.GetInt();
            PreperationPhaseStartedOnTick = reader.GetInt();
            PreperationPhaseEndedOnTick = reader.GetInt();
            IsInPreparationPhase = reader.GetBool();
            IsInShowoffWinners = reader.GetBool();
            CurrentStageWinnerTeamId = reader.GetByte();
            MapSizeMultiplier = reader.GetFloat16();
        }

        public PlayerStateS2C GetPlayerById(ushort playerId)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Id == playerId)
                {
                    return Players.GetByIndex(i);
                } 
            }

            throw new System.Exception($"No player for id {playerId}!");
        }
        
        public PlayerStateS2C GetPlayerByName(string playerName) // one day this will be replaced with device Unique Id. Until then- players must have different names
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name == playerName)
                {
                    return Players.GetByIndex(i);
                } 
            }

            throw new System.Exception($"No player for name {playerName}!");
        }

        public PlayerStateS2C GetPlayerByIndex(int index)
        {
            return Players.GetByIndex(index);
        }

        public void RemoveBulletById(ushort bulletId)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (Bullets[i].Id == bulletId)
                {
                    Bullets.RemoveAt(i);
                    return;
                } 
            }
            
            throw new System.Exception($"No bullet for id {bulletId}!");
        }

        public bool GetIsTalentCurrentlyActiveForPlayer(ushort playerId, TalentType talentType)
        {
            if (GetPlayerById(playerId).Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent))
            {
                return selectedTalent.TalentType == talentType && selectedTalent.IsCurrentlyActive;
            }

            return false;
        }

        public void SetIsTalentCurrentlyActiveForPlayer(ushort playerId, TalentType talentType, bool isActive)
        {
            GetPlayerById(playerId).Spaceship.TalentsState.TrySetIsTalentActive(talentType, isActive);
        }

        public bool GetIsPowerUpCurrentlyActiveForPlayer(ushort playerId)
        {
            return GetPlayerById(playerId).Spaceship.IsPowerUpCurrentlyActive;
        }

        public void SetIsPowerUpCurrentlyActiveForPlayer(ushort playerId, bool isActive)
        {
            GetPlayerById(playerId).Spaceship.IsPowerUpCurrentlyActive = isActive;
        }

        public bool GetIsTalentAimingForPlayer(ushort playerId, TalentType talentType)
        {
            if (GetPlayerById(playerId).Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent))
            {
                return selectedTalent.TalentType == talentType && selectedTalent.IsCurrentlyAiming;
            }

            return false;
        }

        public void SetIsTalentCurrentlyAimingForPlayer(ushort playerId, TalentType talentType, bool isActive)
        {
            GetPlayerById(playerId).Spaceship.TalentsState.TrySetIsTalentAiming(talentType, isActive);
        }
        
        public ref PlayerBulletS2C GetBulletById(ushort bulletId)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (Bullets[i].Id == bulletId)
                {
                    return ref Bullets.Get(i);
                } 
            }
            
            throw new System.Exception($"No bullet for id {bulletId}!");
        }
        
        public bool TryGetBulletById(ushort bulletId, out PlayerBulletS2C bulletState)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                bulletState = Bullets[i];
                if (bulletState.Id == bulletId)
                {
                    return true;
                } 
            }

            bulletState = default;
            return false;
        }
        
        public bool TryGetBulletIndexById(ushort bulletId, out int  index)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (Bullets[i].Id == bulletId)
                {
                    index = i;
                    return true;
                } 
            }

            index = -1;
            return false;
        }
        
        public ref PlayerBulletS2C GetBulletByIndex(int index)
        {
            return ref Bullets.Get(index);
        }

        public void RemoveTalentCardById(ushort cardId)
        {
            for (int i = 0; i < TalentCards.Count; i++)
            {
                if (TalentCards[i].Id == cardId)
                {
                    TalentCards.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No talent card for id {cardId}!");
        }
        
        public ref TalentCardS2C GetTalentCardById(ushort cardId)
        {
            for (int i = 0; i < TalentCards.Count; i++)
            {
                if (TalentCards[i].Id == cardId)
                {
                    return ref TalentCards.GetByIndex(i);
                }
            }

            throw new System.Exception($"No talent card for id {cardId}!");
        }
        
        public ref TalentSwapFieldS2C GetSwapFieldById(ushort swapFieldId)
        {
            for (int i = 0; i < SwapFields.Count; i++)
            {
                if (SwapFields[i].Id == swapFieldId)
                {
                    return ref SwapFields.GetByIndex(i);
                }
            }

            throw new System.Exception($"No swap field for id {swapFieldId}!");
        }
        
        public bool TryGetSwapFieldById(ushort swapFieldId, out TalentSwapFieldS2C swapField)
        {
            for (int i = 0; i < SwapFields.Count; i++)
            {
                if (SwapFields[i].Id == swapFieldId)
                {
                    swapField= SwapFields.GetByIndex(i);
                    return true;
                }
            }

            swapField = default;
            return false;
        }

        public bool TryGetKOProjectileById(ushort koProjectileId, out TalentKOProjectileS2C koProjectile)
        {
            for (int i = 0; i < KOProjectiles.Count; i++)
            {
                if (KOProjectiles[i].Id == koProjectileId)
                {
                    koProjectile = KOProjectiles.GetByIndex(i);
                    return true;
                }
            }

            koProjectile = default;
            return false;
        }
        
        public ref TalentKOProjectileS2C GetKOProjectileById(ushort koProjectileId)
        {
            for (int i = 0; i < KOProjectiles.Count; i++)
            {
                if (KOProjectiles[i].Id == koProjectileId)
                {
                    return ref KOProjectiles.GetByIndex(i);
                }
            }

            throw new System.Exception($"No ko projectile for id {koProjectileId}!");
        }

        public bool TryGetGrapplingHookProjectileById(ushort projectileId, out TalentGrapplingHookProjectileStateS2C projectile)
        {
            for (int i = 0; i < GrapplingHookProjectiles.Count; i++)
            {
                if (GrapplingHookProjectiles[i].Id == projectileId)
                {
                    projectile = GrapplingHookProjectiles.GetByIndex(i);
                    return true;
                }
            }

            projectile = default;
            return false;
        }

        public ref TalentGrapplingHookProjectileStateS2C GetGrapplingHookProjectileById(ushort projectileId)
        {
            for (int i = 0; i < GrapplingHookProjectiles.Count; i++)
            {
                if (GrapplingHookProjectiles[i].Id == projectileId)
                {
                    return ref GrapplingHookProjectiles.GetByIndex(i);
                }
            }

            throw new System.Exception($"No grappling hook projectile for id {projectileId}!");
        }

        public bool TryGetFishingRodProjectileById(ushort projectileId, out TalentFishingRodProjectileStateS2C projectile)
        {
            for (int i = 0; i < FishingRodProjectiles.Count; i++)
            {
                if (FishingRodProjectiles[i].Id == projectileId)
                {
                    projectile = FishingRodProjectiles.GetByIndex(i);
                    return true;
                }
            }

            projectile = default;
            return false;
        }

        public ref TalentFishingRodProjectileStateS2C GetFishingRodProjectileById(ushort projectileId)
        {
            for (int i = 0; i < FishingRodProjectiles.Count; i++)
            {
                if (FishingRodProjectiles[i].Id == projectileId)
                {
                    return ref FishingRodProjectiles.GetByIndex(i);
                }
            }

            throw new System.Exception($"No fishing rod projectile for id {projectileId}!");
        }

        public bool TryGetSoulGhostById(ushort ghostId, out TalentSoulGhostStateS2C soulGhost)
        {
            for (int i = 0; i < SoulGhosts.Count; i++)
            {
                if (SoulGhosts[i].Id == ghostId)
                {
                    soulGhost = SoulGhosts.GetByIndex(i);
                    return true;
                }
            }

            soulGhost = default;
            return false;
        }

        public ref TalentSoulGhostStateS2C GetSoulGhostById(ushort ghostId)
        {
            for (int i = 0; i < SoulGhosts.Count; i++)
            {
                if (SoulGhosts[i].Id == ghostId)
                {
                    return ref SoulGhosts.GetByIndex(i);
                }
            }

            throw new System.Exception($"No soul ghost for id {ghostId}!");
        }

        public bool TryGetTalentCardById(ushort cardId, out TalentCardS2C talentCard)
        {
            for (int i = 0; i < TalentCards.Count; i++)
            {
                talentCard = TalentCards[i];
                if (talentCard.Id == cardId)
                {
                    return true;
                }
            }

            talentCard = default;
            return false;
        }
        
        public bool TryGetTalentCardIndexById(ushort cardId, out int index)
        {
            for (int i = 0; i < TalentCards.Count; i++)
            {
                if (TalentCards[i].Id == cardId)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
        
        public void SerializeDeltas(NetDataWriter writer)
        {
            var playerCount = Players.Count;
            writer.Put((byte) playerCount);
            foreach (var player in Players.AsSpan())
            {
                player.SerializeDeltas(writer);
            }

            var powerUpsCount = PowerUpBalls.Count;
            writer.Put((byte) powerUpsCount);
            foreach (var powerUp in PowerUpBalls.AsSpan())
            {
                powerUp.SerializeTransforms(writer);
            }

            var koProjectilesCount = KOProjectiles.Count;
            writer.Put((byte)koProjectilesCount);
            foreach (var koProjectile in KOProjectiles.AsSpan())
            {
                koProjectile.SerializeDelta(writer);
            }

            var grapplingHookProjectilesCount = GrapplingHookProjectiles.Count;
            writer.Put((byte)grapplingHookProjectilesCount);
            foreach (var grapplingHookProjectile in GrapplingHookProjectiles.AsSpan())
            {
                grapplingHookProjectile.SerializeDelta(writer);
            }

            var fishingRodProjectilesCount = FishingRodProjectiles.Count;
            writer.Put((byte)fishingRodProjectilesCount);
            foreach (var fishingRodProjectile in FishingRodProjectiles.AsSpan())
            {
                fishingRodProjectile.SerializeDelta(writer);
            }

            var soulGhostsCount = SoulGhosts.Count;
            writer.Put((byte)soulGhostsCount);
            foreach (var soulGhost in SoulGhosts.AsSpan())
            {
                soulGhost.SerializeDelta(writer);
            }

            var frigidBlocksCount = FrigidBlocks.Count;
            writer.Put((byte)frigidBlocksCount);
            foreach (var frigidBlock in FrigidBlocks.AsSpan())
            {
                frigidBlock.SerializeDelta(writer);
            }

            var scoreGatesCount = ScoreGates.Count;
            writer.Put((byte)scoreGatesCount);
            foreach (var scoreGate in ScoreGates.AsSpan())
            {
                scoreGate.SerializeDelta(writer);
            }
        }

        private void PutBulletTransformsBatched(NetDataWriter writer) // maybe one day this will be used
        {
            var bulletsCount = Bullets.Count;
            writer.Put((byte) bulletsCount);
            if (bulletsCount == 0)
            {
                return;
            }

            ushort prevBulletId = 0; 
            var isFirstBullet = true;
            foreach (var bullet in Bullets.AsSpan())
            {
                if (isFirstBullet)
                {
                    prevBulletId = bullet.Id;
                    writer.Put(prevBulletId);
                    writer.PutVector2Quantized(bullet.Position);
                    isFirstBullet = false;
                }
                else
                {
                    int idDelta = bullet.Id - prevBulletId;
                    prevBulletId = bullet.Id;
                    writer.Put((byte)idDelta);
                    writer.PutVector2Quantized(bullet.Position);
                }
            }
        }
        
        private void GetBulletTransformsBatched(NetDataReader reader)
        {
            var bulletsCount = reader.GetByte();
            Bullets.Clear();
            if (bulletsCount == 0)
            {
                return;
            }

            ushort prevBulletId = 0;
            var isFirstBullet = true;

            for (int i = 0; i < bulletsCount; i++)
            {
                ref var bullet = ref Bullets.AddAndGet();

                if (isFirstBullet)
                {
                    prevBulletId = reader.GetUShort();
                    bullet.Id = prevBulletId;
                    bullet.Position = reader.GetVector2Quantized();
                    isFirstBullet = false;
                }
                else
                {
                    byte idDelta = reader.GetByte();
                    ushort currentId = (ushort)(prevBulletId + idDelta);
            
                    bullet.Id = currentId;
                    bullet.Position = reader.GetVector2Quantized();
                    prevBulletId = currentId; 
                }
            }
        }
        
        public void DeserializeTransforms(NetDataReader reader)
        {
            var playersCount = reader.GetByte();
            Players.Clear();
            for (var i = 0; i < playersCount; i++)
            {
                var player = Players.AddAndGet();
                player.DeserializeDeltas(reader);
            }

            var powerUpsCount = reader.GetByte();
            PowerUpBalls.Clear();
            for (int i = 0; i < powerUpsCount; i++)
            {
                ref var powerUp = ref PowerUpBalls.AddAndGet();
                powerUp.DeserializeTransforms(reader);
            }

            var koProjectilesCount = reader.GetByte();
            KOProjectiles.Clear();
            for (int i = 0; i < koProjectilesCount; i++)
            {
                ref var koProjectile = ref KOProjectiles.AddAndGet();
                koProjectile.DeserializeDelta(reader);
            }

            var grapplingHookProjectilesCount = reader.GetByte();
            GrapplingHookProjectiles.Clear();
            for (int i = 0; i < grapplingHookProjectilesCount; i++)
            {
                ref var grapplingHookProjectile = ref GrapplingHookProjectiles.AddAndGet();
                grapplingHookProjectile.DeserializeDelta(reader);
            }

            var fishingRodProjectilesCount = reader.GetByte();
            FishingRodProjectiles.Clear();
            for (int i = 0; i < fishingRodProjectilesCount; i++)
            {
                ref var fishingRodProjectile = ref FishingRodProjectiles.AddAndGet();
                fishingRodProjectile.DeserializeDelta(reader);
            }

            var soulGhostsCount = reader.GetByte();
            SoulGhosts.Clear();
            for (int i = 0; i < soulGhostsCount; i++)
            {
                ref var soulGhost = ref SoulGhosts.AddAndGet();
                soulGhost.DeserializeDelta(reader);
            }

            var frigidBlocksCount = reader.GetByte();
            FrigidBlocks.Clear();
            for (int i = 0; i < frigidBlocksCount; i++)
            {
                ref var frigidBlock = ref FrigidBlocks.AddAndGet();
                frigidBlock.DeserializeDelta(reader);
            }

            var scoreGatesCount = reader.GetByte();
            ScoreGates.Clear();
            for (int i = 0; i < scoreGatesCount; i++)
            {
                ref var scoreGate = ref ScoreGates.AddAndGet();
                scoreGate.DeserializeDelta(reader);
            }
        }

        public ref PowerUpBallS2C GetPowerUpBallById(ushort powerUpBallId)
        {
            for (int i = 0; i < PowerUpBalls.Count; i++)
            {
                if (PowerUpBalls[i].Id == powerUpBallId)
                {
                    return ref PowerUpBalls.GetByIndex(i);
                }
            }

            throw new System.Exception($"No power ball for id {powerUpBallId}!");
        }
        
        public bool TryGetPowerUpBallIndexById(ushort powerUpBallId, out int powerUpBallIndex)
        {
            powerUpBallIndex = default;
            for (int i = 0; i < PowerUpBalls.Count; i++)
            {
                if (PowerUpBalls[i].Id == powerUpBallId)
                {
                    powerUpBallIndex = i;
                    return true;
                }
            }

            return false;
        }

        public void RemovePowerUpBallById(ushort powerUpBallId)
        {
            for (int i = 0; i < PowerUpBalls.Count; i++)
            {
                if (PowerUpBalls[i].Id == powerUpBallId)
                {
                    PowerUpBalls.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No power up for id {powerUpBallId}!");
        }
        
        public void RemoveSwapFieldById(ushort swapFieldId)
        {
            for (int i = 0; i < SwapFields.Count; i++)
            {
                if (SwapFields[i].Id == swapFieldId)
                {
                    SwapFields.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No swap field for id {swapFieldId}!");
        }

        public void RemoveKOProjectileById(ushort koProjectileId)
        {
            for (int i = 0; i < KOProjectiles.Count; i++)
            {
                if (KOProjectiles[i].Id == koProjectileId)
                {
                    KOProjectiles.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No ko projectile for id {koProjectileId}!");
        }

        public void RemoveGrapplingHookProjectileById(ushort projectileId)
        {
            for (int i = 0; i < GrapplingHookProjectiles.Count; i++)
            {
                if (GrapplingHookProjectiles[i].Id == projectileId)
                {
                    GrapplingHookProjectiles.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No grappling hook projectile for id {projectileId}!");
        }

        public void RemoveFishingRodProjectileById(ushort projectileId)
        {
            for (int i = 0; i < FishingRodProjectiles.Count; i++)
            {
                if (FishingRodProjectiles[i].Id == projectileId)
                {
                    FishingRodProjectiles.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No fishing rod projectile for id {projectileId}!");
        }

        public void RemoveSoulGhostById(ushort ghostId)
        {
            for (int i = 0; i < SoulGhosts.Count; i++)
            {
                if (SoulGhosts[i].Id == ghostId)
                {
                    SoulGhosts.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No soul ghost for id {ghostId}!");
        }

        public bool TryGetFrigidBlockById(ushort blockId, out TalentFrigidBlockStateS2C frigidBlock)
        {
            for (int i = 0; i < FrigidBlocks.Count; i++)
            {
                if (FrigidBlocks[i].Id == blockId)
                {
                    frigidBlock = FrigidBlocks.GetByIndex(i);
                    return true;
                }
            }

            frigidBlock = default;
            return false;
        }

        public ref TalentFrigidBlockStateS2C GetFrigidBlockById(ushort blockId)
        {
            for (int i = 0; i < FrigidBlocks.Count; i++)
            {
                if (FrigidBlocks[i].Id == blockId)
                {
                    return ref FrigidBlocks.GetByIndex(i);
                }
            }

            throw new System.Exception($"No frigid block for id {blockId}!");
        }

        public void RemoveFrigidBlockById(ushort blockId)
        {
            for (int i = 0; i < FrigidBlocks.Count; i++)
            {
                if (FrigidBlocks[i].Id == blockId)
                {
                    FrigidBlocks.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No frigid block for id {blockId}!");
        }


        public bool TryGetChickenEggById(ushort eggId, out TalentChickenEggStateS2C egg)
        {
            for (int i = 0; i < ChickenEggs.Count; i++)
            {
                if (ChickenEggs[i].Id == eggId)
                {
                    egg = ChickenEggs.GetByIndex(i);
                    return true;
                }
            }
            egg = default;
            return false;
        }

        public ref TalentChickenEggStateS2C GetChickenEggById(ushort eggId)
        {
            for (int i = 0; i < ChickenEggs.Count; i++)
            {
                if (ChickenEggs[i].Id == eggId)
                    return ref ChickenEggs.GetByIndex(i);
            }
            throw new System.Exception($"No chicken egg for id {eggId}!");
        }

        public void RemoveChickenEggById(ushort eggId)
        {
            for (int i = 0; i < ChickenEggs.Count; i++)
            {
                if (ChickenEggs[i].Id == eggId)
                {
                    ChickenEggs.RemoveAt(i);
                    return;
                }
            }
            throw new System.Exception($"No chicken egg for id {eggId}!");
        }

        public bool TryGetPlayerByName(string playerName, out PlayerStateS2C playerState)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name == playerName)
                {
                    playerState = Players.GetByIndex(i);
                    return true;
                } 
            }

            playerState = default;
            return false;
        }

        public void RemoveGalacticForceFieldById(ushort fieldId)
        {
            for (int i = 0; i < GalacticForceFields.Count; i++)
            {
                if (GalacticForceFields[i].Id == fieldId)
                {
                    GalacticForceFields.RemoveAt(i);
                    return;
                }
            }
            throw new System.Exception($"No galactic force field for id {fieldId}!");
        }

        public bool TryGetMoleIndexById(ushort moleId, out int moleIndex)
        {
            for (int i = 0; i < Moles.Count; i++)
            {
                if (Moles[i].Id == moleId)
                {
                    moleIndex = i;
                    return true;
                }
            }

            moleIndex = -1;
            return false;
        }

        public bool TryGetMoleById(ushort moleId, out MoleStateS2C mole)
        {
            for (int i = 0; i < Moles.Count; i++)
            {
                if (Moles[i].Id == moleId)
                {
                    mole = Moles.GetByIndex(i);
                    return true;
                }
            }

            mole = default;
            return false;
        }

        public ref MoleStateS2C GetMoleById(ushort moleId)
        {
            for (int i = 0; i < Moles.Count; i++)
            {
                if (Moles[i].Id == moleId)
                {
                    return ref Moles.GetByIndex(i);
                }
            }

            throw new System.Exception($"No mole for id {moleId}!");
        }

        public void RemoveMoleById(ushort moleId)
        {
            for (int i = 0; i < Moles.Count; i++)
            {
                if (Moles[i].Id == moleId)
                {
                    Moles.RemoveAt(i);
                    return;
                }
            }

            throw new System.Exception($"No mole for id {moleId}!");
        }

        public bool TryGetScoreGateIndexById(ushort scoreGateId, out int scoreGateIndex)
        {
            for (int i = 0; i < ScoreGates.Count; i++)
            {
                if (ScoreGates[i].Id == scoreGateId)
                {
                    scoreGateIndex = i;
                    return true;
                }
            }

            scoreGateIndex = -1;
            return false;
        }

        public ref ScoreGateStateS2C GetScoreGateById(ushort scoreGateId)
        {
            for (int i = 0; i < ScoreGates.Count; i++)
            {
                if (ScoreGates[i].Id == scoreGateId)
                {
                    return ref ScoreGates.GetByIndex(i);
                }
            }

            throw new System.Exception($"No score gate for id {scoreGateId}!");
        }

        public ref EnvironmentGateTrapStateS2C GetGateTrapById(ushort gateTrapId)
        {
            for (int i = 0; i < GateTraps.Count; i++)
            {
                if (GateTraps[i].Id == gateTrapId)
                {
                    return ref GateTraps.GetByIndex(i);
                }
            }

            throw new System.Exception($"No gate trap for id {gateTrapId}!");
        }

        public void AddMolesHitForTeam(ushort teamId, int molesHitDelta)
        {
            MolesHitPerTeamId[teamId] += molesHitDelta;
        }

        public void ResetMolesHitPerTeam(HashSet<ushort> teamIds)
        {
            foreach (var teamId in teamIds)
            {
                MolesHitPerTeamId[teamId] = 0;
            }
        }

        public int AddMolesHitScoreForPlayer(ushort playerId, int scoreDelta)
        {
            var player = GetPlayerById(playerId);
            player.MolesHitScore += scoreDelta;
            return player.MolesHitScore;
        }

        public void ResetMolesHitScoreForAllPlayers()
        {
            foreach (var player in Players.AsSpan())
            {
                player.MolesHitScore = 0;
            }
        }

        public void ClearObjectStates()
        {
            Bullets.Clear();
            PowerUpBalls.Clear();
            TalentCards.Clear();
            SwapFields.Clear();
            KOProjectiles.Clear();
            GrapplingHookProjectiles.Clear();
            FishingRodProjectiles.Clear();
            SoulGhosts.Clear();
            FrigidBlocks.Clear();
            ChickenEggs.Clear();
            GalacticForceFields.Clear();
            Moles.Clear();
            ScoreGates.Clear();
            GateTraps.Clear();
            FieldBarriersOrderedByTeamId.Clear();
        }
    }
}