using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService
{
    public interface IMatchDataService 
    {
        List<MatchPlayerModel> Players { get; }
        List<MatchPlayerBulletModel> Bullets { get; }
        List<MatchPowerUpBallModel> PowerUpBalls { get; }
        List<MatchEnvironmentRotatingWheelModel> RotatingWheels { get; }
        List<MatchEnvironmentTeleportPairModel> EnvironmentTeleportPairs { get; }
        List<MatchEnvironmentSpikeModel> EnvironmentSpikes { get; }
        List<MatchEnvironmentSpringModel> EnvironmentSprings { get; }
        List<MatchEnvironmentWallModel> EnvironmentWalls { get; }
        List<MatchEnvironmentLavaWallModel> EnvironmentLavaWalls { get; }
        List<MatchEnvironmentFieldBarrierModel> FieldBarriers { get; }
        List<MatchSwapFieldModel> SwapFields { get; }
        HashSet<ushort> TeamIds {get; }
        int PreperationPhaseStartedOnTick { get; set; }
        int PreperationPhaseEndedOnTick { get; set; }
        bool IsInPreparationPhase { get; set; }
        public bool IsInShowoffWinners { get; set; }
        public ushort CurrentStageWinnerTeamId { get; set; }
        public StageType StageType { get; set; }
        List<MatchKOProjectileModel> KOProjectiles { get; }
        List<MatchGrapplingHookProjectileModel> GrapplingHookProjectiles { get; }
        List<MatchFishingRodTipModel> FishingRodTips { get; }
        List<MatchSoulGhostModel> SoulGhosts { get; }
        List<MatchFrigidBlockModel> FrigidBlocks { get; }
        Dictionary<ushort, int> BoltsPerTeam  {get; }
        Dictionary<ushort, int> GemsPerTeam  {get; }
        void AddTeamIdIdDoesntExist(ushort teamId);
        MatchPlayerModel GetPlayer(ushort playerId);
        ushort GetPlayerTeamId(ushort playerId);
        MatchPlayerModel AddPlayer(PlayerStateS2C playerState);
        MatchEnvironmentWallModel AddWall(ushort id, Vector2[] points, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle);
        MatchEnvironmentLavaWallModel AddLavalWall(ushort id, Vector2[] points, Vector2 localPosition, Vector2 worldPosition, float worldRotationAngle);
        MatchEnvironmentFieldBarrierModel AddFieldBarrier(ushort id, ushort teamId, Vector2 position, Vector2 size, FieldBarrierShape shape);
        MatchPlayerBulletModel AddBullet(ushort bulletId, ushort belongToPlayerId, Vector2 initialPosition, System.Numerics.Vector2 velocity, float radius, int spawnTick);
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
        MatchEnvironmentSpikeModel AddSpike(ushort id, Vector2 localPosition, Vector2 worldPosition, float localRotationAngle, float worldRotationAngle);
        MatchEnvironmentSpikeModel GetEnvironmentSpike(ushort spikeId);
        MatchEnvironmentFieldBarrierModel GetFieldBarrier(ushort id);
        MatchEnvironmentRotatingWheelModel AddEnvironmentRotatingWheel(ushort id, Vector2 centerPosition, float rotationSpeed, List<ushort> wallIds, List<ushort> lavaWallIds, List<ushort> springIds, List<ushort> spikeIds, List<RotatingTeleportGate> teleportGates);
        void ClearAll();
        void SetTeamBolts(ushort teamId, int totalTeamBolts);
        void SetTeamGems(ushort teamId, int totalTeamGems);
        bool IsTeamLeadingInGems(ushort teamId);
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
        MatchFishingRodTipModel AddFishingRodTip(ushort id, ushort casterPlayerId, Vector2 position);
        MatchFishingRodTipModel GetFishingRodTip(ushort id);
        void RemoveFishingRodTip(ushort id);
        MatchSoulGhostModel AddSoulGhost(ushort id, ushort casterPlayerId, Vector2 position, Vector2 direction);
        MatchSoulGhostModel GetSoulGhost(ushort id);
        void RemoveSoulGhost(ushort id);
        MatchFrigidBlockModel AddFrigidBlock(ushort id, ushort casterPlayerId, Vector2 position, Vector2 rotation);
        void RemoveFrigidBlock(ushort id);
        MatchKOProjectileModel GetKOProjectile(ushort id);
        void RemoveKOProjectile(ushort id);
        MatchChickenEggModel GetChickenEgg(ushort id);
        MatchChickenEggModel AddChickenEgg(ushort id, ushort casterPlayerId, UnityEngine.Vector2 position);
        void RemoveChickenEgg(ushort id);
        bool TryGetKingedPlayers(out List<MatchPlayerModel> kingedPlayers);
    }
}