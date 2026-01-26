using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService
{
    public interface IMatchMakingDataService
    {
        List<MatchMakingPlayerModel> Players { get; }
        List<MatchPlayerBulletModel> Bullets { get; }
        MatchMakingPlayerModel GetPlayer(ushort playerId);
        MatchMakingPlayerModel LocalPlayer { get; }
        bool IsPlayerJoined { get; }
        MatchMakingPlayerModel AddPlayer(MatchMakingPlayerStateS2C playerState);
        MatchEnvironmentWallModel AddWall(WallConfig wallConfig);
        void SetLocalPlayer(int playerId);
        MatchPlayerBulletModel AddBullet(ushort bulletId, ushort belongToPlayerId, System.Numerics.Vector2 position, float radius);
        MatchPlayerBulletModel GetBullet(ushort bulletId);
        MatchEnvironmentWallModel GetEnvironmentWall(ushort wallId);
        void RemoveBullet(ushort bulletId);
        void UpdatePlayerTeam(ushort playerId, int teamId);
    }
}
