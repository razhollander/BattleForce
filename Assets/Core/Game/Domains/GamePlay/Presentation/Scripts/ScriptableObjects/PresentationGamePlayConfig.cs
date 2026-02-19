using ASoliman.Utils.EditableRefs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PresentationGamePlayConfig", menuName = "BF/Presentation/GamePlay Config")]
    public class PresentationGamePlayConfig : ScriptableObject
    {
        //public float InterpolationFactor = 0.85f;
        public float ExponentialDecay = 15; // usefull range 1-25, 1=slow, 25=fast
        [EditableRef] public TalentCardsConfig TalentCards;
        [EditableRef] public PowerUpsConfig PowerUps;
        [EditableRef] public TeamFloorConfig TeamFloor;
        [EditableRef] public EnvironmentSpringConfig EnvironmentSpring;
        public SerializableDictionary<int, Color> ColorPerTeamId;
    }
}