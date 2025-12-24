using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.MatchData.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using CoreDomain.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        public List<MatchPlayerModel> Players { get; private set; }
        public List<MatchPlayerBulletModel> Bullets { get; private set; }
        public List<MatchEnvironmentWallModel> EnvironmentWalls { get; private set; }

        public MatchPlayerModel LocalPlayer { get; private set; }
        public bool IsPlayerJoined => LocalPlayer != null;

        public MatchDataService(NetworkConfig networkConfig)
        {
            Players = new List<MatchPlayerModel>(networkConfig.MaxConnectedPlayers);
            Bullets = new List<MatchPlayerBulletModel>(networkConfig.MaxConcurrentBullets);
            EnvironmentWalls = new List<MatchEnvironmentWallModel>();
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

        public MatchEnvironmentWallModel AddWall(EnvironmentWallStateS2C wallState)
        {
            var newWall = new MatchEnvironmentWallModel(wallState.Id, wallState.Points.Select(x=>x.ToUnityVector2()).ToArray());
            EnvironmentWalls.Add(newWall);
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