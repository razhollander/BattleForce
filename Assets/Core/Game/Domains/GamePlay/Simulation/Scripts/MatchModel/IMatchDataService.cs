using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        SimulationStateS2C SimulationState { get; }
        //SimulationStateS2C PreviousSimulationState { get; }
        ref PlayerStateS2C AddPlayer(string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius, ushort health,
            float shootCooldown);
        PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius);
        //void CopySimulationStateIntoPrevious();
    }
}