using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public interface ISimulationInputService
    {
        void SetInputs(ushort playerId, InputType inputType, bool isPressed);
        bool WasInputDownThisTick(ushort playerId, InputType inputType);
        bool WasInputReleasedThisTick(ushort playerId, InputType inputType);
        bool IsInputPressed(ushort playerId, InputType inputType);
    }
}