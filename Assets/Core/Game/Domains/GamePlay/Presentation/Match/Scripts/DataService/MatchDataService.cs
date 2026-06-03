using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService
{
    public class MatchDataService : IMatchDataService
    {
        public List<MatchPlayerModel> Players { get; private set; }
        public List<MatchPlayerBulletModel> Bullets { get; private set; }
        public List<MatchEnvironmentWallModel> EnvironmentWalls { get; private set; }
        public List<MatchEnvironmentLavaWallModel> EnvironmentLavaWalls { get; private set; }
        public List<MatchEnvironmentSpringModel> EnvironmentSprings { get; private set; }
        public List<MatchEnvironmentTeleportPairModel> EnvironmentTeleportPairs { get; private set; }
        public List<MatchEnvironmentRotatingWheelModel> RotatingWheels { get; private set; }
        public List<MatchEnvironmentFieldBarrierModel> FieldBarriers { get; private set; }
        public List<MatchSwapFieldModel> SwapFields { get; private set; }
        public List<MatchKOProjectileModel> KOProjectiles { get; private set; }
        public List<MatchGrapplingHookProjectileModel> GrapplingHookProjectiles { get; private set; }
        public List<MatchTalentCardModel> TalentCards { get; private set; }
        public List<MatchPowerUpBallModel> PowerUpBalls { get; private set; }
        public List<MatchChickenEggModel> ChickenEggs { get; private set; }

        public MatchPlayerModel LocalPlayer { get; private set; }
        public bool IsPlayerJoined => LocalPlayer != null;
        public HashSet<ushort> TeamIds  {get; private set; }
        public int StartPhaseInitialTick { get; set; }
        public bool IsInPreparationPhase { get; set; }
        public Dictionary<ushort, int> BoltsPerTeam  {get; private set; }
        public Dictionary<ushort, int> GemsPerTeam  {get; private set; }
        public MatchDataService(NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            Players = new List<MatchPlayerModel>(networkConfig.MaxCap.ConcurrentPlayers);
            Bullets = new List<MatchPlayerBulletModel>(networkConfig.MaxCap.ConcurrentBullets);
            EnvironmentWalls = new List<MatchEnvironmentWallModel>(networkConfig.MaxCap.ConcurrentEvironmentWalls);
            EnvironmentLavaWalls = new List<MatchEnvironmentLavaWallModel>(networkConfig.MaxCap.ConcurrentEvironmentLavaWalls);
            EnvironmentSprings = new List<MatchEnvironmentSpringModel>(networkConfig.MaxCap.ConcurrentEvironmentSprings);
            RotatingWheels = new List<MatchEnvironmentRotatingWheelModel>(networkConfig.MaxCap.ConcurrentEnvironmentRotatingWheels);
            TalentCards = new List<MatchTalentCardModel>(networkConfig.MaxCap.ConcurrentTalentCards);
            PowerUpBalls = new List<MatchPowerUpBallModel>(networkConfig.MaxCap.ConcurrentPowerUpBalls);
            TeamIds = new HashSet<ushort>(sharedGamePlayConfig.MaxTeamsAmount);
            BoltsPerTeam = new Dictionary<ushort, int>(sharedGamePlayConfig.MaxTeamsAmount);
            GemsPerTeam = new Dictionary<ushort, int>(sharedGamePlayConfig.MaxTeamsAmount);
            EnvironmentTeleportPairs = new List<MatchEnvironmentTeleportPairModel>(networkConfig.MaxCap.ConcurrentEvironmentTeleportPairs);
            FieldBarriers = new List<MatchEnvironmentFieldBarrierModel>(networkConfig.MaxCap.ConcurrentFieldBarriers);
            SwapFields = new List<MatchSwapFieldModel>(networkConfig.MaxCap.ConcurrentPlayers);
            KOProjectiles = new List<MatchKOProjectileModel>(networkConfig.MaxCap.ConcurrentPlayers);
            GrapplingHookProjectiles = new List<MatchGrapplingHookProjectileModel>(networkConfig.MaxCap.ConcurrentPlayers);
            ChickenEggs = new List<MatchChickenEggModel>(networkConfig.MaxCap.ConcurrentChickenEggs);
        }

        public MatchPlayerBulletModel GetBullet(ushort bulletId)
        {
            return Bullets.Find(x => x.Id == bulletId);
        }

        public MatchEnvironmentWallModel GetEnvironmentWall(ushort wallId)
        {
            return EnvironmentWalls.Find(x => x.Id == wallId);
        }
        
        public MatchEnvironmentLavaWallModel GetEnvironmentLavaWall(ushort wallId)
        {
            return EnvironmentLavaWalls.Find(x => x.Id == wallId);
        }

        public MatchEnvironmentSpringModel GetEnvironmentSpring(ushort springId)
        {
            return EnvironmentSprings.Find(x => x.Id == springId);
        }

        public void RemoveBullet(ushort bulletId)
        {
            var bullet = Bullets.Find(x => x.Id == bulletId);

            if (bullet == null)
            {
                LogService.LogError($"No bullet to remove with id {bulletId}!");
                return;
            }
            
            Bullets.Remove(bullet);
        }

        public MatchTalentCardModel GetTalentCard(ushort cardId)
        {
            return TalentCards.Find(x => x.Id == cardId);
        }

        public MatchTalentCardModel AddTalentCard(ushort talentCardId, UnityEngine.Vector2 talentCardPosition, TalentType talentCardTalentType, ushort talentCardHealth)
        {
            var newTalentCard = new MatchTalentCardModel(talentCardId, talentCardPosition, talentCardTalentType, talentCardHealth);
            TalentCards.Add(newTalentCard);
            return newTalentCard;
        }

        public MatchPowerUpBallModel GetPowerUpBall(ushort powerUpBallId)
        {
            return PowerUpBalls.Find(x => x.Id == powerUpBallId);
        }

        public MatchPowerUpBallModel AddPowerUpBall(ushort powerUpBallId, UnityEngine.Vector2 position)
        {
            var newPowerUpBall = new MatchPowerUpBallModel(powerUpBallId, position);
            PowerUpBalls.Add(newPowerUpBall);
            return newPowerUpBall;
        }

        public void RemoveTalentCard(ushort cardId)
        {
            var talentCardModel = TalentCards.Find(x => x.Id == cardId);

            if (talentCardModel == null)
            {
                LogService.LogError($"No talent card to remove with id {cardId}!");
                return;
            }
            
            TalentCards.Remove(talentCardModel);
        }
        
        public void RemovePowerUpBall(ushort powerUpBallId)
        {
            var powerUpBallModel = GetPowerUpBall(powerUpBallId);

            if (powerUpBallModel == null)
            {
                LogService.LogError($"No power up ball to remove with id {powerUpBallId}!");
                return;
            }
            
            PowerUpBalls.Remove(powerUpBallModel);
        }

        public MatchPlayerModel GetPlayer(ushort playerId)
        {
            return Players.Find(x => x.PlayerId == playerId);
        }
        
        public ushort GetPlayerTeamId(ushort playerId)
        {
            return Players.Find(x => x.PlayerId == playerId).TeamId;
        }

        public MatchPlayerModel AddPlayer(PlayerStateS2C playerState)
        {
            var playerTeamId = playerState.TeamId;
            var newPlayer = new MatchPlayerModel(playerState.Id, playerState.Name, playerTeamId, playerState.Spaceship);
            Players.Add(newPlayer);
            TeamIds.Add(playerTeamId);
            BoltsPerTeam.TryAdd(playerTeamId, 0);
            GemsPerTeam.TryAdd(playerTeamId, 0);
            return newPlayer;
        }

        public MatchEnvironmentWallModel AddWall(ushort id, Vector2[] points, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle)
        {
            var newWall = new MatchEnvironmentWallModel(id, points, localPosition, worldPosition, worldRotationAngle);
            EnvironmentWalls.Add(newWall);
            return newWall;
        }

        public MatchEnvironmentLavaWallModel AddLavalWall(ushort id, Vector2[] points, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle)
        {
            var newWall = new MatchEnvironmentLavaWallModel(id, points, localPosition, worldPosition, worldRotationAngle);
            EnvironmentLavaWalls.Add(newWall);
            return newWall;
        }

        public MatchEnvironmentFieldBarrierModel AddFieldBarrier(ushort id, ushort teamId, Vector2 position, Vector2 size, FieldBarrierShape shape)
        {
            var newBarrier = new MatchEnvironmentFieldBarrierModel(id, teamId, position, size, shape);
            FieldBarriers.Add(newBarrier);
            return newBarrier;
        }

        public MatchEnvironmentSpringModel AddSpring(ushort id, Vector2 localPosition, Vector2 worldPosition, float localRotationAngle, float worldRotationAngle)
        {
            var newSpring = new MatchEnvironmentSpringModel(id, localPosition, worldPosition, localRotationAngle, worldRotationAngle);
            EnvironmentSprings.Add(newSpring);
            return newSpring;
        }

        public MatchEnvironmentRotatingWheelModel AddEnvironmentRotatingWheel(EnvironmentRotatingWheelConfig config)
        {
            var newWheel = new MatchEnvironmentRotatingWheelModel(config.Id, config.CenterPosition, config.RotationSpeed, 
                config.Walls.IsNullOrEmpty() ? new List<ushort>() : config.Walls.Select(x=>x.Id).ToList(), 
                config.LavaWalls.IsNullOrEmpty() ? new List<ushort>() : config.LavaWalls.Select(x=>x.Id).ToList(), 
                config.Springs.IsNullOrEmpty() ? new List<ushort>() : config.Springs.Select(x=>x.Id).ToList(),
                config.TeleportGatePairs.IsNullOrEmpty() ? new List<ushort>() : config.TeleportGatePairs.Select(x=>x.Id).ToList());
            RotatingWheels.Add(newWheel);
            return newWheel;
        }

        public MatchPlayerBulletModel AddBullet(ushort bulletId, ushort belongToPlayerId, Vector2 initialPosition, System.Numerics.Vector2 velocity, float radius, int spawnTick)
        {
            var newBullet = new MatchPlayerBulletModel(bulletId, belongToPlayerId, initialPosition, velocity, radius, spawnTick);
            Bullets.Add(newBullet);
            return newBullet;
        }

        public void SetLocalPlayer(int playerId)
        {
            LocalPlayer = Players.Find(x => x.PlayerId == playerId);
        }

        public void ClearAll()
        {
            Players.Clear();
            Bullets.Clear();
            EnvironmentWalls.Clear();
            EnvironmentLavaWalls.Clear();
            EnvironmentSprings.Clear();
            RotatingWheels.Clear();
            TalentCards.Clear();
            PowerUpBalls.Clear();
            BoltsPerTeam.Clear();
            GemsPerTeam.Clear();
            EnvironmentTeleportPairs.Clear();
            FieldBarriers.Clear();
            SwapFields.Clear();
            KOProjectiles.Clear();
            GrapplingHookProjectiles.Clear();
            ChickenEggs.Clear();
        }

        public void SetTeamBolts(ushort teamId, int totalTeamBolts)
        {
            BoltsPerTeam[teamId] = totalTeamBolts;
        }

        public void SetTeamGems(ushort teamId, int totalTeamGems)
        {
            GemsPerTeam[teamId] = totalTeamGems;
        }

        public void AddTeleportPair(ushort teleportPairId, ushort gateAId, Vector2 gateAPosition, float gateANormalRotation, ushort gateBId, Vector2 gateBPosition, float gateBNormalRotation, Vector2 gateAWorldPosition, float gateAWorldRotation, Vector2 gateBWorldPosition, float gateBWorldRotation, Vector2 size)
        {
            var teleportPairModel = new MatchEnvironmentTeleportPairModel(teleportPairId, gateAId, gateAPosition, gateANormalRotation, gateBId, gateBPosition, gateBNormalRotation, gateAWorldPosition, gateAWorldRotation, gateBWorldPosition, gateBWorldRotation, size);
            EnvironmentTeleportPairs.Add(teleportPairModel);
        }

        public MatchEnvironmentTeleportPairModel GetTeleportPair(ushort teleportPairId)
        {
            return EnvironmentTeleportPairs.Find(x => x.Id == teleportPairId);
        }

        public MatchSwapFieldModel AddSwapField(ushort id, ushort casterPlayerId, int startTick, int endTick, float maxRadius)
        {
            var swapFieldModel = new MatchSwapFieldModel(id, casterPlayerId, startTick, endTick, maxRadius);
            SwapFields.Add(swapFieldModel);
            return swapFieldModel;
        }

        public MatchSwapFieldModel GetSwapField(ushort id)
        {
            var swapField = SwapFields.Find(x => x.Id == id);
            if (swapField == null)
            {
                LogService.LogError($"Couldn't find swap field with id {id}");
            }
            
            return swapField;
        }

        public void RemoveSwapField(ushort id)
        {
            var swapFieldModel = GetSwapField(id);

            if (swapFieldModel == null)
            {
                LogService.LogError($"No swap field to remove with id {id}!");
                return;
            }
            
            SwapFields.Remove(swapFieldModel);
        }

        public MatchEnvironmentFieldBarrierModel GetFieldBarrier(ushort id)
        {
            var fieldBarrier = FieldBarriers.Find(x => x.Id == id);
            if (fieldBarrier == null)
            {
                LogService.LogError($"Couldn't find field barrier with id {id}");
            }
            
            return fieldBarrier;
        }

        public MatchKOProjectileModel AddKOProjectile(ushort id, ushort casterPlayerId, int startTick, float size)
        {
            var model = new MatchKOProjectileModel(id, casterPlayerId, startTick, size);
            KOProjectiles.Add(model);
            return model;
        }

        public MatchGrapplingHookProjectileModel AddGrapplingHookProjectile(ushort id, ushort casterPlayerId, Vector2 position)
        {
            var model = new MatchGrapplingHookProjectileModel(id, casterPlayerId, position.ToUnityVector2());
            GrapplingHookProjectiles.Add(model);
            return model;
        }

        public MatchGrapplingHookProjectileModel GetGrapplingHookProjectile(ushort id)
        {
            var model = GrapplingHookProjectiles.Find(x => x.Id == id);
            if (model == null)
            {
                LogService.LogError($"Couldn't find grappling hook projectile with id {id}");
            }
            return model;
        }

        public void RemoveGrapplingHookProjectile(ushort id)
        {
            for (int i = 0; i < GrapplingHookProjectiles.Count; i++)
            {
                if (GrapplingHookProjectiles[i].Id == id)
                {
                    GrapplingHookProjectiles.RemoveAt(i);
                    return;
                }
            }
        }

        public MatchKOProjectileModel GetKOProjectile(ushort id)
        {
            var model = KOProjectiles.Find(x => x.Id == id);
            if (model == null)
            {
                LogService.LogError($"Couldn't find ko projectile with id {id}");
            }
            return model;
        }

        public void RemoveKOProjectile(ushort id)
        {
            var model = GetKOProjectile(id);

            if (model == null)
            {
                LogService.LogError($"No ko projectile to remove with id {id}!");
                return;
            }

            KOProjectiles.Remove(model);
        }

        public MatchChickenEggModel GetChickenEgg(ushort id)
        {
            var model = ChickenEggs.Find(x => x.Id == id);
            if (model == null)
            {
                LogService.LogError($"Couldn't find chieck egg with id {id}");
            }

            return model;
        }

        public MatchChickenEggModel AddChickenEgg(ushort id, ushort casterPlayerId, UnityEngine.Vector2 position)
        {
            var model = new MatchChickenEggModel
            {
                Id = id,
                CasterPlayerId = casterPlayerId,
                Position = position.ToNumericsVector2(),
            };
            
            ChickenEggs.Add(model);
            return model;
        }

        public void RemoveChickenEgg(ushort id)
        {
            var model = GetChickenEgg(id);

            if (model == null)
            {
                LogService.LogError($"No chieck egg to remove with id {id}!");
                return;
            }

            ChickenEggs.Remove(model);
        }
    }
}