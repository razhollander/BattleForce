using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnvironmentTeleportConfig", menuName = "BF/Presentation/Environment Teleport Config")]
    public class EnvironmentTeleportConfig : ScriptableObject
    {
        [SerializeField]
        public SerializableDictionary<ushort, Sprite> TeleportSpritesPerId = new SerializableDictionary<ushort, Sprite>();
    }
}