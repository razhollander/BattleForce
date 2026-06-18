using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using Core.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Scripts.Services.HapticsService
{
    public class HapticsService : IHapticsService
    {
        [DllImport("MacRumblePlugin")]
        private static extern void PlayMacBluetoothRumble(int index, float lowFreq, float highFreq);
        [DllImport("MacRumblePlugin")]
        private static extern void ResetMacBluetoothRumble();
        private readonly HapticsConfig _hapticsConfig;
        private readonly Dictionary<Gamepad, CancellationTokenSource> _activeHapticsTasks = new();

        public HapticsService(HapticsConfig hapticsConfig)
        {
            _hapticsConfig = hapticsConfig;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            ResetMacBluetoothRumble();
#endif
        }

        public void PlayHaptics(HapticType hapticType, Gamepad gamepad)
        {
            PlayOneShotRoutineAsync(gamepad, _hapticsConfig.Profiles[hapticType], new CancellationToken()).Forget();
        }

        private async Awaitable PlayOneShotRoutineAsync(Gamepad gamepad, HapticsProfile profile, CancellationToken token)
        {
           
            int gamepadIndex = Gamepad.all.IndexOf(g => g == gamepad);            
            if (gamepadIndex < 0) gamepadIndex = 0; 
            
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            PlayMacBluetoothRumble(gamepadIndex, profile.LowFrequency, profile.HighFrequency);
#else
            gamepad.SetMotorSpeeds(profile.LowFrequency, profile.HighFrequency);
#endif

            try
            {
                await Awaitable.WaitForSecondsAsync(profile.Duration, token);
            }
            finally
            {
                if (gamepad != null)
                {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                    PlayMacBluetoothRumble(gamepadIndex, 0f, 0f);
#else
                    gamepad.SetMotorSpeeds(0f, 0f);
#endif
                    _activeHapticsTasks.Remove(gamepad);
                }
            }
        }

        public void StopGamepadHaptics(Gamepad gamepad)
        {
            if (gamepad == null) return;

            if (_activeHapticsTasks.TryGetValue(gamepad, out var activeTask))
            {
                activeTask?.Cancel();
                activeTask?.Dispose();
                _activeHapticsTasks.Remove(gamepad);
            }
            
            int gamepadIndex = Gamepad.all.IndexOf(g => g == gamepad);
            if (gamepadIndex < 0) gamepadIndex = 0;

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            PlayMacBluetoothRumble(gamepadIndex, 0f, 0f);
#else
            gamepad.SetMotorSpeeds(0f, 0f);
#endif
        }
    }
}