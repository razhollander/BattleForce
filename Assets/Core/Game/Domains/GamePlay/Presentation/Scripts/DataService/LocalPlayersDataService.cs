using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.DataService
{
    public class LocalPlayersDataService : ILocalPlayersDataService
    {
        private readonly List<ushort> _localPlayerIds = new List<ushort>();
        private readonly Dictionary<ushort, InputDevice> _deviceByPlayerId = new Dictionary<ushort, InputDevice>();

        public IReadOnlyList<ushort> LocalPlayerIds => _localPlayerIds;

        public void AddLocalPlayer(ushort playerId, InputDevice device)
        {
            if (!_localPlayerIds.Contains(playerId))
            {
                _localPlayerIds.Add(playerId);
            }
            _deviceByPlayerId[playerId] = device;
        }

        public void RemoveLocalPlayer(ushort playerId)
        {
            _localPlayerIds.Remove(playerId);
            _deviceByPlayerId.Remove(playerId);
        }

        public InputDevice GetInputDeviceForPlayer(ushort playerId)
        {
            if (_deviceByPlayerId.TryGetValue(playerId, out var device))
            {
                return device;
            }
            return null;
        }
    }
}
