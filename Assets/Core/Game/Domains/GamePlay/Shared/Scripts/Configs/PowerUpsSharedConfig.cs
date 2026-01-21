using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "PowerUpsSharedConfig", menuName = "BF/Shared/Power Ups Config")]
    public class PowerUpsSharedConfig : ScriptableObject
    {
        [SerializeField] public float PowerUpObtainDelayInSeconds = 2f;
    }
}