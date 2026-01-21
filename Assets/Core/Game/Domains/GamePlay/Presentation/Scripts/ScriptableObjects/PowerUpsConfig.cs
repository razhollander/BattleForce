using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PowerUpsConfig", menuName = "BF/Presentation/PowerUps Config")]
    public class PowerUpsConfig: ScriptableObject
    {
        [SerializeField] public SerializableDictionary<PowerUpType, Sprite> PowerUpSprites = new SerializableDictionary<PowerUpType, Sprite>();
    }
}