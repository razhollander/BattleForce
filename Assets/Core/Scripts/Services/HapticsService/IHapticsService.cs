using UnityEngine.InputSystem;

namespace Core.Scripts.Services.HapticsService
{
    public interface IHapticsService
    {
        void PlayHaptics(HapticType hapticType, Gamepad gamepad);
    }
}