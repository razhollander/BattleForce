using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public interface IPowerUpController
    {
        PowerUpType PowerUpType { get; }
        void SetCasterId(ushort casterPlayerId);
        void Perform(int tick);
        void OnTick(int tick);
        void Reset();
    }
}
