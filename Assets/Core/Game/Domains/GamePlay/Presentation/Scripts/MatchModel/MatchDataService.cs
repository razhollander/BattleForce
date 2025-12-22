using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.MatchData.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public class MatchDataService : IMatchDataService
    {
        public List<MatchPlayerModel> Players { get; private set; }
        public List<MatchPlayerBulletModel> Bullets { get; private set; }

        public MatchPlayerModel LocalPlayer { get; private set; }
        public bool IsPlayerJoined => LocalPlayer != null;

        public MatchDataService(NetworkConfig networkConfig)
        {
            Players = new List<MatchPlayerModel>(networkConfig.MaxConnectedPlayers);
            Bullets = new List<MatchPlayerBulletModel>(networkConfig.MaxConcurrentBullets);
        }

        public MatchPlayerBulletModel GetBullet(ushort bulletId)
        {
            return Bullets.Find(x => x.Id == bulletId);
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