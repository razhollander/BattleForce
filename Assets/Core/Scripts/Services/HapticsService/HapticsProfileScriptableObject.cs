using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Scripts.Services.HapticsService
{
    [CreateAssetMenu(fileName = "HapticsProfiles", menuName = "BF/Presentation//Haptics Profiles")]
    public class HapticsProfileScriptableObject : ScriptableObject
    {
        [SerializeField] 
        public SerializableDictionary<HapticProfileType, HapticsProfile> Profiles = new SerializableDictionary<HapticProfileType, HapticsProfile>();
    }
}