using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        SimulationStateS2C SimulationState { get; }
        SimulationStateS2C PreviousSimulationState { get; }
        PlayerStateS2C AddPlayer(string playerName, PlayerTransformStateS2C playerTransformStateS2C, int health,
            float shootCooldown);
        PlayerStateS2C GetPlayer(int playerId);
        void SetPlayer(int playerId, PlayerStateS2C playerModel);
        PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius);
        void CopySimulationStateIntoPrevious();
        void SetBullet(ushort bulletModelId, PlayerBulletS2C bulletModel);
        PlayerBulletS2C GetBullet(int bulletId);
        void RemoveBullet(ushort bulletModelId);
    }
}