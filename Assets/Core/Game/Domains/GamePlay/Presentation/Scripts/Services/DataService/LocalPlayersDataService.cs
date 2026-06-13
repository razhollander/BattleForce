using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService
{
    public class LocalPlayersDataService : ILocalPlayersDataService
    {
        private readonly List<ushort> _localPlayerIds = new List<ushort>();
        private readonly Dictionary<ushort, InputDevice> _deviceByPlayerId = new Dictionary<ushort, InputDevice>();
        private readonly Dictionary<ushort, int> _deviceIdByPlayerId = new Dictionary<ushort, int>();

        public List<ushort> LocalPlayersIds => _localPlayerIds;
        public bool IsClientJoined => LocalPlayersIds.Count > 0;

        public Dictionary<ushort, int> GetPlayerIdToDeviceIdDictionary()
        {
            return _deviceIdByPlayerId;
        }
        
        public void AddLocalPlayer(ushort playerId, InputDevice device)
        {
            if (!_localPlayerIds.Contains(playerId))
            {
                _localPlayerIds.Add(playerId);
            }
            _deviceByPlayerId[playerId] = device;
            _deviceIdByPlayerId[playerId] = device.deviceId;
        }

        public void RemoveLocalPlayer(ushort playerId)
        {
            _localPlayerIds.Remove(playerId);
            _deviceByPlayerId.Remove(playerId);
            _deviceIdByPlayerId.Remove(playerId);
        }

        public InputDevice GetInputDeviceForPlayer(ushort playerId)
        {
            if (_deviceByPlayerId.TryGetValue(playerId, out var device))
            {
                return device;
            }
            return null;
        }

        public void SetLocalPlayers(Dictionary<ushort, int> playerIdToDeviceIdDictionary)
        {
            _localPlayerIds.Clear();
            _deviceByPlayerId.Clear();
            _localPlayerIds.Clear();
            foreach (var kvp in playerIdToDeviceIdDictionary)
            {
                var device = InputSystem.GetDeviceById(kvp.Value);
                var isPlayerAddedBySkippingMatchMaking = device == null;
                if(isPlayerAddedBySkippingMatchMaking)
                {
                    continue;
                }
                
                AddLocalPlayer(kvp.Key, device);
            }
        }
    }
}