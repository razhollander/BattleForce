using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService
{
    public interface IMatchDataService 
    {
        List<MatchPlayerModel> Players { get; }
        List<MatchPlayerBulletModel> Bullets { get; }
        List<MatchPowerUpBallModel> PowerUpBalls { get; }
        List<MatchEnvironmentTeleportPairModel> EnvironmentTeleportPairs { get; }
        HashSet<ushort> TeamIds {get; }
        MatchPlayerModel GetPlayer(ushort playerId);
        ushort GetPlayerTeamId(ushort playerId);
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
        MatchEnvironmentSpringModel AddSpring(ushort id, UnityEngine.Vector2 position, float directionAngle);
        MatchEnvironmentSpringModel GetEnvironmentSpring(ushort springId);
        void ClearAll();
        void SetTeamBolts(ushort teamId, int totalTeamBolts);
        void SetTeamGems(ushort teamId, int totalTeamGems);

        void AddTeleportPair(ushort teleportPairId, ushort gateAId, Vector2 gateAPosition, float gateANormalRotation, ushort gateBId, Vector2 gateBPosition,
            float gateBNormalRotation, Vector2 size);

        MatchEnvironmentTeleportPairModel GetTeleportPair(ushort teleportPairId);
    }
}