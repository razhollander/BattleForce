using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchData.Models;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        List<MatchPlayerModel> Players { get; }
        List<MatchPlayerBulletModel> Bullets { get; }
        List<MatchPowerUpBallModel> PowerUpBalls { get; }
        MatchPlayerModel GetPlayer(ushort playerId);
        MatchPlayerModel LocalPlayer { get; }
        bool IsPlayerJoined { get; }
        MatchPlayerModel AddPlayer(PlayerStateS2C playerState);
        MatchEnvironmentWallModel AddWall(WallConfig wallConfig);
        MatchEnvironmentLavaWallModel AddLavalWall(WallConfig wallConfig);
        void SetLocalPlayer(int playerId);
        MatchPlayerBulletModel AddBullet(ushort bulletId, ushort belongToPlayerId, Vector2 position, float radius);
        MatchPlayerBulletModel GetBullet(ushort bulletId);
        MatchEnvironmentWallModel GetEnvironmentWall(ushort wallId);
        void RemoveBullet(ushort bulletId);
        MatchTalentCardModel GetTalentCard(ushort cardId);
        MatchTalentCardModel AddTalentCard(ushort talentCardId, UnityEngine.Vector2 talentCardPosition, TalentType talentCardTalentType, ushort talentCardHealth);
        void RemoveTalentCard(ushort cardId);
        MatchPowerUpBallModel GetPowerUpBall(ushort powerUpBallId);
        MatchPowerUpBallModel AddPowerUpBall(ushort powerUpBallId, UnityEngine.Vector2 position);
        void RemovePowerUpBall(ushort powerUpBallId);
        MatchEnvironmentLavaWallModel GetEnvironmentLavaWall(ushort lavaWallId);
    }
}