using Core.Scripts.Utils.CustomCollections;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public class SimulationInputService : ISimulationInputService
    {
        private readonly CapacityDict<ushort, InputState> _inputStates;

        public SimulationInputService(NetworkConfig networkConfig)
        {
            _inputStates = new CapacityDict<ushort, InputState>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void SetInputs(ushort playerId, InputType inputType, bool isPressed)
        {
            InputState inputState;
            if (!_inputStates.TryGetValue(playerId, out inputState))
            {
                inputState = new InputState();
                _inputStates.Add(playerId, inputState);
            }

            switch (inputType)
            {
                case InputType.SwitchTalent:
                    inputState.SwitchTalent.Update(isPressed);
                    break;
            }
        }

        public bool WasInputDownThisTick(ushort playerId, InputType inputType)
        {
            if (!_inputStates.TryGetValue(playerId, out var inputState))
            {
                return false;
            }

            switch (inputType)
            {
                case InputType.SwitchTalent:
                    return inputState.SwitchTalent.WasDownThisTick;
                default:
                    return false;
            }
        }

        public bool WasInputReleasedThisTick(ushort playerId, InputType inputType)
        {
            if (!_inputStates.TryGetValue(playerId, out var inputState))
            {
                return false;
            }

            switch (inputType)
            {
                case InputType.SwitchTalent:
                    return inputState.SwitchTalent.WasReleasedThisTick;
                default:
                    return false;
            }
        }

        public bool IsInputPressed(ushort playerId, InputType inputType)
        {
            if (!_inputStates.TryGetValue(playerId, out var inputState))
            {
                return false;
            }

            switch (inputType)
            {
                case InputType.SwitchTalent:
                    return inputState.SwitchTalent.IsPressed;
                default:
                    return false;
            }
        }

        private class InputState
        {
            public SingleInputState SwitchTalent = new SingleInputState();
        }

        private class SingleInputState
        {
            public bool IsPressed;
            public bool WasDownThisTick;
            public bool WasReleasedThisTick;

            public void Update(bool isPressed)
            {
                WasDownThisTick = isPressed && !IsPressed;
                WasReleasedThisTick = !isPressed && IsPressed;
                IsPressed = isPressed;
            }
        }
    }
}