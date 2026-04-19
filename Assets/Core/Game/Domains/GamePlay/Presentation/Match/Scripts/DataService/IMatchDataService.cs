using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService
{
    public interface IMatchDataService 
    {
        List<MatchPlayerModel> Players { get; }
        List<MatchPlayerBulletModel> Bullets { get; }
        List<MatchPowerUpBallModel> PowerUpBalls { get; }
        List<MatchEnvironmentRotatingWheelModel> RotatingWheels { get; }
        List<MatchEnvironmentTeleportPairModel> EnvironmentTeleportPairs { get; }
        List<MatchEnvironmentFieldBarrierModel> FieldBarriers { get; }
        List<MatchSwapFieldModel> SwapFields { get; }
        HashSet<ushort> TeamIds {get; }
        int StartPhaseInitialTick { get; set; }
        bool IsInPreparationPhase { get; set; }
        List<MatchKOProjectileModel> KOProjectiles { get; }
        List<MatchGrapplingHookProjectileModel> GrapplingHookProjectiles { get; }
        MatchPlayerModel LocalPlayer { get; }
        bool IsPlayerJoined { get; }
        
        MatchPlayerModel GetPlayer(ushort playerId);
        ushort GetPlayerTeamId(ushort playerId);
        MatchPlayerModel AddPlayer(PlayerStateS2C playerState);
        MatchEnvironmentWallModel AddWall(ushort id, Vector2[] points, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle);
        MatchEnvironmentLavaWallModel AddLavalWall(ushort id, Vector2[] points, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle);
        MatchEnvironmentFieldBarrierModel AddFieldBarrier(ushort id, ushort teamId, Vector2 position, Vector2 size, FieldBarrierShape shape);
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
        MatchEnvironmentSpringModel AddSpring(ushort id, Vector2 localPosition, Vector2 worldPosition, float localRotationAngle, float worldRotationAngle);
        MatchEnvironmentSpringModel GetEnvironmentSpring(ushort springId);
        MatchEnvironmentFieldBarrierModel GetFieldBarrier(ushort id);
        MatchEnvironmentRotatingWheelModel AddEnvironmentRotatingWheel(EnvironmentRotatingWheelConfig config);
        void ClearAll();
        void SetTeamBolts(ushort teamId, int totalTeamBolts);
        void SetTeamGems(ushort teamId, int totalTeamGems);
        void AddTeleportPair(ushort teleportPairId, ushort gateAId, Vector2 gateAPosition, float gateANormalRotation, ushort gateBId, Vector2 gateBPosition,
            float gateBNormalRotation, Vector2 gateAWorldPosition, float gateAWorldRotation, Vector2 gateBWorldPosition, float gateBWorldRotation, Vector2 size);
        MatchEnvironmentTeleportPairModel GetTeleportPair(ushort teleportPairId);
        MatchSwapFieldModel AddSwapField(ushort id, ushort casterPlayerId, int startTick, int endTick, float maxRadius);
        MatchSwapFieldModel GetSwapField(ushort id);
        void RemoveSwapField(ushort id);
        MatchKOProjectileModel AddKOProjectile(ushort id, ushort casterPlayerId, int startTick, float size);
        MatchGrapplingHookProjectileModel AddGrapplingHookProjectile(ushort id, ushort casterPlayerId, Vector2 position);
        MatchGrapplingHookProjectileModel GetGrapplingHookProjectile(ushort id);
        void RemoveGrapplingHookProjectile(ushort id);
        MatchKOProjectileModel GetKOProjectile(ushort id);
        void RemoveKOProjectile(ushort id);
        Core.Game.Domains.GamePlay.Shared.S2CModels.ChickenEggModelS2C GetChickenEgg(ushort id);
        void AddChickenEgg(ushort id, UnityEngine.Vector2 position, bool isBroken);
        void BreakChickenEgg(ushort id);
        void RemoveChickenEgg(ushort id);
    }
}