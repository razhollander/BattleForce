using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService
{
    public class MatchMakingDataService : IMatchMakingDataService
    {
        public List<MatchMakingPlayerModel> Players { get; private set; }
        public List<MatchPlayerBulletModel> Bullets { get; private set; }
        public List<MatchEnvironmentWallModel> EnvironmentWalls { get; private set; }

        public MatchMakingDataService(NetworkConfig networkConfig)
        {
            Players = new List<MatchMakingPlayerModel>(networkConfig.MaxCap.ConcurrentPlayers);
            Bullets = new List<MatchPlayerBulletModel>(networkConfig.MaxCap.ConcurrentBullets);
            EnvironmentWalls = new List<MatchEnvironmentWallModel>(networkConfig.MaxCap.ConcurrentEvironmentWalls);
        }

        public MatchPlayerBulletModel GetBullet(ushort bulletId)
        {
            return Bullets.Find(x => x.Id == bulletId);
        }

        public MatchEnvironmentWallModel GetEnvironmentWall(ushort wallId)
        {
            return EnvironmentWalls.Find(x => x.Id == wallId);
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
        
        public MatchMakingPlayerModel GetPlayer(ushort playerId)
        {
            return Players.Find(x => x.PlayerId == playerId);
        }

        public MatchMakingPlayerModel AddPlayer(MatchMakingPlayerStateS2C playerState)
        {
            var newPlayer = new MatchMakingPlayerModel(playerState.Id, playerState.Name, playerState.Spaceship, playerState.TeamId);
            Players.Add(newPlayer);
            return newPlayer;
        }

        public MatchEnvironmentWallModel AddWall(WallConfig wallConfig)
        {
            var newWall = new MatchEnvironmentWallModel(wallConfig.Id, wallConfig.Points, Vector2.Zero,  wallConfig.Position, 0);
            EnvironmentWalls.Add(newWall);
            return newWall;
        }

        public MatchPlayerBulletModel AddBullet(ushort bulletId, ushort belongToPlayerId, Vector2 initialPosition, Vector2 velocity, float radius, int spawnTick)
        {
            var newBullet = new MatchPlayerBulletModel(bulletId, belongToPlayerId, initialPosition, velocity, radius, spawnTick);
            Bullets.Add(newBullet);
            return newBullet;
        }

        public void UpdatePlayerTeam(ushort playerId, ushort teamId)
        {
            var player = GetPlayer(playerId);
            if (player != null)
            {
                player.TeamId = teamId;
            }
        }
    }
}
