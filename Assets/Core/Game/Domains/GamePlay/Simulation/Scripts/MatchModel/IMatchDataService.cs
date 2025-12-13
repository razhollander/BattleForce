using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public interface IMatchDataService 
    {
        SimulationStateS2C SimulationState { get; }
        SimulationStateS2C PreviousSimulationState { get; }
        MatchNetEventsDataService EventsData { get; }
        PlayerStateS2C AddPlayer(string playerName, PlayerTransformStateS2C playerTransformStateS2C, int health,
            float shootCooldown);
        PlayerStateS2C GetPlayer(int playerId);
        void SetPlayer(int playerId, PlayerStateS2C playerModel);
        PlayerBulletS2C AddBullet(int processedTick, ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed);
        void CopySimulationStateIntoPrevious();
        void RemoveAllEventsOlderThanTick(ushort playerId, int tick);
    }
}