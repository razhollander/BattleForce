using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PresentationGamePlayConfig", menuName = "BF/Presentation/GamePlay Config")]
    public class PresentationGamePlayConfig : ScriptableObject
    {
        public float InterpolationFactor = 0.85f;
        public TalentCardsConfig TalentCards;
    }
}