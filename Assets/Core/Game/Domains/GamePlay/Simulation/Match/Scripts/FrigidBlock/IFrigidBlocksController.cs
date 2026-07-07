using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock
{
    public interface IFrigidBlocksController
    {
        void ShootFrigidBlock(ushort casterPlayerId, Vector2 position, Vector2 direction, int tick, int cooldownEndTick);
        void OnTick(int tick, float deltaTime);
        void ResetData();
    }
}
