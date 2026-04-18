using System.Collections.Generic;
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
        public FixedUnorderedList<PlayerBulletS2C> Bullets;
        public FixedUnorderedList<TalentCardS2C> TalentCards;
        public FixedUnorderedList<PowerUpBallS2C> PowerUpBalls;
        public FixedUnorderedList<TalentSwapFieldS2C> SwapFields;
        public FixedUnorderedList<TalentKOProjectileS2C> KOProjectiles;
        public FixedUnorderedList<TalentGrapplingHookProjectileStateS2C> GrapplingHookProjectiles;
        public Dictionary<ushort, int> GemsPerTeamId;
        public Dictionary<ushort, int> BoltsPerTeam;
        public int EnvironmentLayoutId;
        public StageType StageType;
        public int StartPhaseInitialTick;
        public bool IsInPreparationPhase;

        public MatchSimulationStateS2C(int maxPlayers, int maxBullets, int maxTalentsPerPlayer, int maxTalentCards, int maxPowerUpBalls, int maxTeams)
        {
            Players = new FixedClassUnorderedList<PlayerStateS2C>(maxPlayers, ()=>new PlayerStateS2C(maxTalentsPerPlayer));
            Bullets = new FixedUnorderedList<PlayerBulletS2C>(maxBullets);
            TalentCards = new FixedUnorderedList<TalentCardS2C>(maxTalentCards);
            PowerUpBalls = new FixedUnorderedList<PowerUpBallS2C>(maxPowerUpBalls);
            SwapFields = new FixedUnorderedList<TalentSwapFieldS2C>(maxPlayers);
            KOProjectiles = new FixedUnorderedList<TalentKOProjectileS2C>(maxPlayers);
            GrapplingHookProjectiles = new FixedUnorderedList<TalentGrapplingHookProjectileStateS2C>(maxPlayers);
            GemsPerTeamId = new Dictionary<ushort, int>(maxTeams);
            BoltsPerTeam = new Dictionary<ushort, int>(maxTeams);
        }

        public void Serialize(NetDataWriter writer)
        {
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

            writer.Put((ushort)GemsPerTeamId.Count);
            foreach (var kvp in GemsPerTeamId)
            {
                writer.Put(kvp.Key);
                writer.Put(kvp.Value);
            }

            writer.Put((ushort)BoltsPerTeam.Count);
            foreach (var kvp in BoltsPerTeam)
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

            writer.Put((byte)EnvironmentLayoutId);
            writer.Put((byte)StageType);
            writer.Put(StartPhaseInitialTick);
            writer.Put(IsInPreparationPhase);
        }
        
        public void Deserialize(NetDataReader reader)
        {
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
            
            GemsPerTeamId.Clear();
            var jemsCount = reader.GetUShort();
            for (int i = 0; i < jemsCount; i++)
            {
                var teamId = reader.GetUShort();
                var jems = reader.GetInt();
                GemsPerTeamId.Add(teamId, jems);
            }

            BoltsPerTeam.Clear();
            var boltsCount = reader.GetUShort();
            for (int i = 0; i < boltsCount; i++)
            {
                var teamId = reader.GetUShort();
                var bolts = reader.GetInt();
                BoltsPerTeam.Add(teamId, bolts);
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

            EnvironmentLayoutId = reader.GetByte();
            StageType = (StageType)reader.GetByte();
            StartPhaseInitialTick = reader.GetInt();
            IsInPreparationPhase = reader.GetBool();
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
                return selectedTalent.TalentType == talentType && selectedTalent.IsActive;
            }

            return false;
        }

        public void SetIsTalentCurrentlyActiveForPlayer(ushort playerId, TalentType talentType, bool isActive)
        {
            GetPlayerById(playerId).Spaceship.TalentsState.TrySetIsTalentActive(talentType, isActive);
        }
        
        public ref PlayerBulletS2C GetBulletById(ushort bulletId)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (Bullets[i].Id == bulletId)
                {
                    return ref Bullets.GetByIndex(i);
                } 
            }
            
            throw new System.Exception("No bullet for id {playerId}!");
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
            return ref Bullets.GetByIndex(index);
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

            var bulletsCount = Bullets.Count;
            writer.Put((byte) bulletsCount);
            foreach (var bullet in Bullets.AsSpan())
            {
                bullet.SerializeTransforms(writer);
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

            var bulletsCount = reader.GetByte();
            Bullets.Clear();
            for (int i = 0; i < bulletsCount; i++)
            {
                ref var bullet = ref Bullets.AddAndGet();
                bullet.DeserializeTransforms(reader);
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

        public  bool TryGetPlayerByName(string playerName, out PlayerStateS2C playerState)
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
    }
}