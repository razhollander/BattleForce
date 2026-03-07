using Core.Scripts.Utils.CustomCollections;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public class SimulationInputService : ISimulationInputService
    {
        private readonly CapacityDict<ushort, PlayerInputStates> _inputStatesPerPlayer;

        public SimulationInputService(NetworkConfig networkConfig)
        {
            _inputStatesPerPlayer = new CapacityDict<ushort, PlayerInputStates>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void AddPlayer(ushort playerId)
        {
            if (_inputStatesPerPlayer.ContainsKey(playerId))
            {
                return;
            }

            var playerInputStates = new PlayerInputStates();
            _inputStatesPerPlayer.Add(playerId, playerInputStates);
        }

        public void SetPlayerInput(ushort playerId, PlayerInputType inputType, bool isPressed)
        {
            if (!_inputStatesPerPlayer.TryGetValue(playerId, out var inputStates))
            {
                LogService.LogError($"InputState not found for playerId: {playerId}");
                return;
            }

            switch (inputType)
            {
                case PlayerInputType.SwitchTalent:
                    inputStates.SwitchTalent.Update(isPressed);
                    break;
            }
        }

        public bool WasInputDownThisTick(ushort playerId, PlayerInputType inputType)
        {
            if (!_inputStatesPerPlayer.TryGetValue(playerId, out var inputStates))
            {
                LogService.LogError($"InputState not found for playerId: {playerId}");
                return false;
            }

            switch (inputType)
            {
                case PlayerInputType.SwitchTalent:
                    return inputStates.SwitchTalent.WasDownThisTick;
                default:
                    return false;
            }
        }

        public bool WasInputReleasedThisTick(ushort playerId, PlayerInputType inputType)
        {
            if (!_inputStatesPerPlayer.TryGetValue(playerId, out var inputStates))
            {
                LogService.LogError($"InputState not found for playerId: {playerId}");
                return false;
            }

            switch (inputType)
            {
                case PlayerInputType.SwitchTalent:
                    return inputStates.SwitchTalent.WasReleasedThisTick;
                default:
                    return false;
            }
        }

        public bool IsInputPressed(ushort playerId, PlayerInputType inputType)
        {
            if (!_inputStatesPerPlayer.TryGetValue(playerId, out var inputStates))
            {
                LogService.LogError($"InputState not found for playerId: {playerId}");
                return false;
            }

            switch (inputType)
            {
                case PlayerInputType.SwitchTalent:
                    return inputStates.SwitchTalent.IsPressed;
                default:
                    return false;
            }
        }

        private class PlayerInputStates
        {
            public TickInputState SwitchTalent = new TickInputState();
        }

        private struct TickInputState
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