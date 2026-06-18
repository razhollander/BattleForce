using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService
{
    public interface ILocalPlayersDataService
    {
        bool IsClientJoined { get; }
        List<ushort> LocalPlayersIds { get; }
        bool TryGetLocalPlayerInputDevice(ushort playerId, out InputDevice inputDevice);
        Dictionary<ushort, int> GetPlayerIdToDeviceIdDictionary();
        void AddLocalPlayer(ushort playerId, InputDevice device);
        void RemoveLocalPlayer(ushort playerId);
        InputDevice GetInputDeviceForPlayer(ushort playerId);
        void SetLocalPlayers(Dictionary<ushort,int> playerIdToDeviceIdDictionary);
    }
}