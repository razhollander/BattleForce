using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentSpikeConfig", menuName = "BF/Shared/Environment Spikes Config")]
    public class EnvironmentSpikesConfig : ScriptableObject
    {
        public ushort Damage = 10;
        public Vector2 Size = new Vector2(0.5f, 1f);
    }
}
