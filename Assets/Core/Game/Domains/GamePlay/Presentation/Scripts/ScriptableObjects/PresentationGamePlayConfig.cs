using ASoliman.Utils.EditableRefs;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PresentationGamePlayConfig", menuName = "BF/Presentation/GamePlay Config")]
    public class PresentationGamePlayConfig : ScriptableObject
    {
        public float InterpolationFactor = 0.85f;
        [EditableRef] public TalentCardsConfig TalentCards;
        [EditableRef] public PowerUpsConfig PowerUps;
        [EditableRef] public TeamFloorConfig TeamFloor;
        public SerializableDictionary<int, Color> ColorPerTeamId;
    }
}