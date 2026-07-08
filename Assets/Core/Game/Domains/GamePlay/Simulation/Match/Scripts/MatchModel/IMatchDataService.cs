using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        MatchSimulationStateS2C SimulationState { get; }
        MatchEnvironmentDataService EnvironmentData { get; }
        List<int> DidntPlayYetStageIndexes { get;  }
        HashSet<ushort> TeamIds { get; }
        //SimulationStateS2C PreviousSimulationState { get; }
        PlayerStateS2C AddPlayer(ushort playerId, ushort teamId, string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius, ushort health,
            float shootCooldown);
        PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius, int createdOnTick);
        //void CopySimulationStateIntoPrevious();
        TalentCardS2C AddTalentCard(ushort talentCardId, Vector2 position, TalentType talentType, ushort health);
        PowerUpBallS2C AddPowerUpBall(Vector2 position, Vector2 velocity, PowerUpType powerUpType);
        TalentSwapFieldS2C AddSwapField(ushort casterPlayerId, int tick, int fieldEndTick);
        TalentKOProjectileS2C AddKOProjectile(int tick, ushort casterPlayerId, Vector2 transformPosition, Vector2 rotation, Vector2 velocity, float koConfigProjectileSize);
        TalentGrapplingHookProjectileStateS2C AddGrapplingHookProjectile(ushort casterPlayerId, Vector2 transformPosition, Vector2 velocity);
        TalentFishingRodProjectileStateS2C AddFishingRodProjectile(ushort casterPlayerId, Vector2 transformPosition, Vector2 velocity);
        TalentFrigidBlockStateS2C AddFrigidBlock(ushort casterPlayerId, Vector2 position, Vector2 rotation, Vector2 velocity);
        TalentChickenEggStateS2C AddChickenEgg(ushort casterPlayerId, Vector2 position);
        GalacticForceFieldS2C AddGalacticForceField(ushort casterTeamId, int endTick);
    }
}