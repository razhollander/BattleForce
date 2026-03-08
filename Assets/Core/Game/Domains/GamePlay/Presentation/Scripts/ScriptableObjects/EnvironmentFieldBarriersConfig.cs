using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "FieldBarriersConfig", menuName = "BF/Presentation/Field Barriers Config")]

    public class EnvironmentFieldBarriersConfig : ScriptableObject
    {
        public float Thickness = 0.2f;
        public int CircleBarrierSegmentsAmount = 64;
    }
}