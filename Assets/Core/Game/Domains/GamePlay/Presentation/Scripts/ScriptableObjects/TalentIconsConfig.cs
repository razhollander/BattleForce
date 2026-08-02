using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TalentCardsConfig", menuName = "BF/Presentation/Talent Cards Config")]
    public class TalentIconsConfig : ScriptableObject
    {
        [SerializeField]
        public SerializableDictionary<TalentType, Sprite> TalentSprites = new SerializableDictionary<TalentType, Sprite>();
    }
}
