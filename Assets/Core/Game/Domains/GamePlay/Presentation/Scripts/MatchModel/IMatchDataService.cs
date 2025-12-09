using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.MatchData.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        List<MatchPlayerModel> Players { get; }
        List<MatchPlayerBulletModel> Bullets { get; }
        MatchPlayerModel GetPlayer(int playerId);
        MatchPlayerModel LocalPlayer { get; }
        bool IsPlayerJoined { get; }
        MatchPlayerModel AddPlayer(int playerId, string playerName, PlayerSpaceshipStateS2C spaceshipState);
        void SetLocalPlayer(MatchPlayerModel matchPlayerModel);
        MatchPlayerBulletModel AddBullet(int bulletId, int belongToPlayerId, Vector2 position);
        MatchPlayerBulletModel GetBullet(int bulletId);
    }
}