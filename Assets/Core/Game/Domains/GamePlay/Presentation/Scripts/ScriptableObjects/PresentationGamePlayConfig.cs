using ASoliman.Utils.EditableRefs;
using Core.Scripts.Services.HapticsService;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PresentationGamePlayConfig", menuName = "BF/Presentation/GamePlay Config")]
    public class PresentationGamePlayConfig : ScriptableObject
    {
        //public float InterpolationFactor = 0.85f;
        public float ExponentialDecay = 15; // usefull range 1-25, 1=slow, 25=fast
        [EditableRef] public TalentIconsConfig TalentCards;
        [EditableRef] public TalentsConfig TalentsConfig;
        [EditableRef] public TeamFloorConfig TeamFloor;
        [EditableRef] public EnvironmentTeleportConfig Teleports;
        [EditableRef] public EnvironmentFieldBarriersConfig FieldBarriers;
        [EditableRef] public HapticsConfig HapticsConfig;
        public bool ShouldOverrideClientId = false;
        [EnableIf(nameof(ShouldOverrideClientId))]
        public long ClientIdOverride = 1;
        public SerializableDictionary<int, Color> ColorPerTeamId;
    }
}