using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public interface IInputDeviceChangedListenerService
    {
        event Action<Gamepad> GamepadAddedEvent;
        event Action<Gamepad> GamepadRemovedEvent;
        void InitEntryPoint();
        void InitExitPoint();
        List<Keyboard> GetAllConnectedKeyboards();
        List<Gamepad> GetAllConnectedGamepads();
    }
}