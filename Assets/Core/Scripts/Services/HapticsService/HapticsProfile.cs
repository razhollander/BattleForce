using System;
using UnityEngine;

namespace Core.Scripts.Services.HapticsService
{
    [Serializable]
    public class HapticsProfile
    {
        [Tooltip("Deep, heavy rumble (e.g., explosions, heavy impacts)")]
        [Range(0f, 1f)] 
        public float LowFrequency;

        [Tooltip("Sharp, subtle tingling (e.g., UI clicks, light scraping)")]
        [Range(0f, 1f)] 
        public float HighFrequency;

        [Tooltip("Duration in seconds for OneShot play types")]
        [Min(0.01f)] 
        public float Duration;
    }
}