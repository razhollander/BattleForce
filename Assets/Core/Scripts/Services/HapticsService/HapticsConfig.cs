using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Scripts.Services.HapticsService
{
    [CreateAssetMenu(fileName = "HapticsProfiles", menuName = "BF/Presentation/Haptics Profiles")]
    public class HapticsConfig : ScriptableObject
    {
        [SerializeField] 
        public SerializableDictionary<HapticType, HapticsProfile> Profiles = new SerializableDictionary<HapticType, HapticsProfile>();
    }
}