using System;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TalentsConfig", menuName = "BF/Presentation/Talents Config")]
    public class TalentsConfig : ScriptableObject
    {
        [SerializeField]
        public SerializableDictionary<TalentType, TalentConfig> Talents = new SerializableDictionary<TalentType, TalentConfig>();
    }

    [Serializable]
    public class TalentConfig
    {
        public bool IsArrowShownWhileSelected;
        public bool IsArrowShownWhileActive;
        public bool IsArrowShownOnlyWhilePressed;
        public bool IsFrontArrow;
    }
}