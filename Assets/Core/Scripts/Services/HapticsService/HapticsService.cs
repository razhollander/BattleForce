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
            // TEMP DIAGNOSTIC — remove once controller identity is confirmed.
            // Dumps the target pad and every connected pad (name / deviceId / Gamepad.all index / serial)
            // so we can verify whether identical pads expose a distinct serial we can match on.
            // var sb = new System.Text.StringBuilder();
            // var targetIndex = Gamepad.all.IndexOf(g => g == gamepad);
            // sb.Append($"[HapticsDiag] TARGET name='{gamepad?.name}' deviceId={gamepad?.deviceId} allIndex={targetIndex} serial='{gamepad?.description.serial}' product='{gamepad?.description.product}' manufacturer='{gamepad?.description.manufacturer}'\n");
            // for (var i = 0; i < Gamepad.all.Count; i++)
            // {
            //     var g = Gamepad.all[i];
            //     sb.Append($"[HapticsDiag]   all[{i}] name='{g.name}' deviceId={g.deviceId} serial='{g.description.serial}' product='{g.description.product}'\n");
            // }
            // UnityEngine.Debug.LogWarning(sb.ToString());

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