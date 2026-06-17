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
        // 1. Update the DllImport to accept an integer index
        [DllImport("MacRumblePlugin")]
        private static extern void PlayMacBluetoothRumble(int index, float lowFreq, float highFreq);

        private readonly HapticsProfileScriptableObject _hapticsProfileScriptableObject;
        private readonly Dictionary<Gamepad, CancellationTokenSource> _activeHapticsTasks = new();

        public HapticsService(HapticsProfileScriptableObject hapticsProfileScriptableObject)
        {
            _hapticsProfileScriptableObject = hapticsProfileScriptableObject;
        }

        public void PlayHaptics(HapticProfileType hapticProfileType, Gamepad gamepad)
        {
            PlayOneShotRoutineAsync(gamepad, _hapticsProfileScriptableObject.Profiles[hapticProfileType], CancellationToken.None).Forget();
        }

        private async Awaitable PlayOneShotRoutineAsync(Gamepad gamepad, HapticsProfile profile, CancellationToken token)
        {
            // 2. Find the index of this specific gamepad in Unity's hardware list
// To this:
            int gamepadIndex = Gamepad.all.IndexOf(g => g == gamepad);            
            // Failsafe: If gamepad isn't found, default to 0
            if (gamepadIndex < 0) gamepadIndex = 0; 

            // 3. Pass the index into the Mac rumble function
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
                    // 4. Pass the index to stop the specific gamepad
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
            
            // 5. Find the index for the safety stop method
// To this:
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