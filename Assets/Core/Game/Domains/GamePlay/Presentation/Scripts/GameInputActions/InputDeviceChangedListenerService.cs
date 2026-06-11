using System;
using System.Collections.Generic;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine.InputSystem;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions
{
    public class InputDeviceChangedListenerService : IInputDeviceChangedListenerService
    {
        public event Action<Gamepad> GamepadAddedEvent;
        public event Action<Gamepad> GamepadRemovedEvent;
        
        public void InitEntryPoint()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not Gamepad gamepad)
            {
                return;
            }
            
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Enabled:
                    GamepadAddedEvent?.Invoke(gamepad);
                    break;
                case InputDeviceChange.Disabled:
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    GamepadRemovedEvent?.Invoke(gamepad); 
                    break;
            }
        }

        public List<Gamepad> GetAllConnectedGamepads()
        {
            var connectedGamepads = new List<Gamepad>();
            
            foreach (InputDevice device in InputSystem.devices)
            {
                if (device is Gamepad gamepad)
                {
                    connectedGamepads.Add(gamepad);
                }
            }

            return connectedGamepads;
        }
        
        public List<Keyboard> GetAllConnectedKeyboards()
        {
            var connectedKeyboards = new List<Keyboard>();
            
            foreach (InputDevice device in InputSystem.devices)
            {
                if (device is Keyboard keyboard)
                {
                    connectedKeyboards.Add(keyboard);
                }
            }

            return connectedKeyboards;
        }

        public void InitExitPoint()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }
}