using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.DataService
{
    public interface ILocalPlayersDataService
    {
        IReadOnlyList<ushort> LocalPlayerIds { get; }
        void AddLocalPlayer(ushort playerId, InputDevice device);
        void RemoveLocalPlayer(ushort playerId);
        InputDevice GetInputDeviceForPlayer(ushort playerId);
    }
}
