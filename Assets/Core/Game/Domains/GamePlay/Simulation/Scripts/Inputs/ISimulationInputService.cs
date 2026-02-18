namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public interface ISimulationInputService
    {
        void AddPlayer(ushort playerId);
        void SetPlayerInput(ushort playerId, PlayerInputType inputType, bool isPressed);
        bool WasInputDownThisTick(ushort playerId, PlayerInputType inputType);
        bool WasInputReleasedThisTick(ushort playerId, PlayerInputType inputType);
        bool IsInputPressed(ushort playerId, PlayerInputType inputType);
    }
}