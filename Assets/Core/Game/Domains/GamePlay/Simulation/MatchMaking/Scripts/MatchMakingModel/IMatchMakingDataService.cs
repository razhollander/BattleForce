using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel
{
    public interface IMatchMakingDataService
    {
        void InitEntryPoint();
        MatchMakingSimulationStateS2C SimulationState { get; }
        MatchMakingEnvironmentDataService Environment { get; }
        MatchMakingPlayerStateS2C AddPlayer(string playerName, Vector2 position, Vector2 direction, Vector2 velocity, float radius, float shootCooldown, ushort teamId);
        PlayerBulletS2C AddBullet(ushort belongToPlayerId, Vector2 position, Vector2 direction, float moveSpeed, float radius);
    }
}