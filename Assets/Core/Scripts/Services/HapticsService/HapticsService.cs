using System.Collections.Generic;
using System.Threading;
using Core.Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Scripts.Services.HapticsService
{
    public class HapticsService : IHapticsService
    {
        private readonly HapticsProfileScriptableObject _hapticsProfileScriptableObject;
        private readonly Dictionary<Gamepad, CancellationTokenSource> _activeHapticsTasks = new();

        public HapticsService(HapticsProfileScriptableObject hapticsProfileScriptableObject)
        {
            _hapticsProfileScriptableObject = hapticsProfileScriptableObject;
        }

        public void PlayHaptics(HapticProfileType hapticProfileType, Gamepad gamepad)
        {
            if (TryPrepareHaptics(hapticProfileType, gamepad, out var profile))
            {
                var cts = new CancellationTokenSource();
                _activeHapticsTasks[gamepad] = cts;

                PlayOneShotRoutineAsync(gamepad, profile, cts.Token).Forget();
            }
        }

        private bool TryPrepareHaptics(HapticProfileType hapticProfileType, Gamepad gamepad, out HapticsProfile profile)
        {
            profile = default;

            if (!TryGetHapticsProfile(hapticProfileType, out profile))
            {
                return false;
            }

            StopGamepadHaptics(gamepad);
            return true;
        }

        private async Awaitable PlayOneShotRoutineAsync(Gamepad gamepad, HapticsProfile profile, CancellationToken token)
        {
            gamepad.SetMotorSpeeds(profile.LowFrequency, profile.HighFrequency);

            try
            {
                await Awaitable.WaitForSecondsAsync(profile.Duration, token);
            }
            finally
            {
                if (gamepad != null)
                {
                    gamepad.SetMotorSpeeds(0f, 0f);
                    _activeHapticsTasks.Remove(gamepad);
                }
            }
        }

        private bool TryGetHapticsProfile(HapticProfileType hapticProfileType, out HapticsProfile profile)
        {
            profile = _hapticsProfileScriptableObject.Profiles[hapticProfileType];
            return profile != null;
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
            
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }
}