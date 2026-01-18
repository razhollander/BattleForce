using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.MatchData.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchData.Models;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        public List<MatchPlayerModel> Players { get; private set; }
        public List<MatchPlayerBulletModel> Bullets { get; private set; }
        public List<MatchEnvironmentWallModel> EnvironmentWalls { get; private set; }
        public List<MatchEnvironmentLavaWallModel> EnvironmentLavaWalls { get; private set; }
        public List<MatchTalentCardModel> TalentCards { get; private set; }

        public MatchPlayerModel LocalPlayer { get; private set; }
        public bool IsPlayerJoined => LocalPlayer != null;

        public MatchDataService(NetworkConfig networkConfig)
        {
            Players = new List<MatchPlayerModel>(networkConfig.MaxCap.ConcurrentPlayers);
            Bullets = new List<MatchPlayerBulletModel>(networkConfig.MaxCap.ConcurrentBullets);
            EnvironmentWalls = new List<MatchEnvironmentWallModel>(networkConfig.MaxCap.ConcurrentEvironmentWalls);
            EnvironmentLavaWalls = new List<MatchEnvironmentLavaWallModel>(networkConfig.MaxCap.ConcurrentEvironmentLavaWalls);
            TalentCards = new List<MatchTalentCardModel>(networkConfig.MaxCap.ConcurrentTalentCards);
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

        public MatchPlayerModel GetPlayer(ushort playerId)
        {
            return Players.Find(x => x.PlayerId == playerId);
        }

        public MatchPlayerModel AddPlayer(PlayerStateS2C playerState)
        {
            var newPlayer = new MatchPlayerModel(playerState.Id, playerState.Name, playerState.Spaceship);
            Players.Add(newPlayer);
            return newPlayer;
        }

        public MatchEnvironmentWallModel AddWall(WallConfig wallConfig)
        {
            var newWall = new MatchEnvironmentWallModel(wallConfig.Id, wallConfig.Points);
            EnvironmentWalls.Add(newWall);
            return newWall;
        }

        public MatchEnvironmentLavaWallModel AddLavalWall(WallConfig wallConfig)
        {
            var newWall = new MatchEnvironmentLavaWallModel(wallConfig.Id, wallConfig.Points);
            EnvironmentLavaWalls.Add(newWall);
            return newWall;
        }

        public MatchPlayerBulletModel AddBullet(ushort bulletId, ushort belongToPlayerId, Vector2 position, float radius)
        {
            var newBullet = new MatchPlayerBulletModel(bulletId, belongToPlayerId, position, radius);
            Bullets.Add(newBullet);
            return newBullet;
        }

        public void SetLocalPlayer(int playerId)
        {
            LocalPlayer = Players.Find(x => x.PlayerId == playerId);
        }
    }
}