using System.Collections.Generic;
using Core.Scripts.Utils.CustomCollections;
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

            inputStates.InputStates[inputType].Update(isPressed);
        }

        public bool WasInputDownThisTick(ushort playerId, PlayerInputType inputType)
        {
            if (!_inputStatesPerPlayer.TryGetValue(playerId, out var inputStates))
            {
                LogService.LogError($"InputState not found for playerId: {playerId}");
                return false;
            }

            return inputStates.InputStates[inputType].WasDownThisTick;
        }

        public bool WasInputReleasedThisTick(ushort playerId, PlayerInputType inputType)
        {
            if (!_inputStatesPerPlayer.TryGetValue(playerId, out var inputStates))
            {
                LogService.LogError($"InputState not found for playerId: {playerId}");
                return false;
            }

            return inputStates.InputStates[inputType].WasReleasedThisTick;
        }

        public bool IsInputPressed(ushort playerId, PlayerInputType inputType)
        {
            if (!_inputStatesPerPlayer.TryGetValue(playerId, out var inputStates))
            {
                LogService.LogError($"InputState not found for playerId: {playerId}");
                return false;
            }

            return inputStates.InputStates[inputType].IsPressed;
        }

        private class PlayerInputStates
        {
            public Dictionary<PlayerInputType, TickInputState> InputStates = new Dictionary<PlayerInputType, TickInputState>
            {
                {PlayerInputType.SwitchTalent, new TickInputState()},
                {PlayerInputType.TalentInput, new TickInputState()},
                {PlayerInputType.Shoot, new TickInputState()}
            };
        }

        private class TickInputState
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