using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentTeleportConfig", menuName = "BF/Shared/Environment Teleport Config")]
    public class EnvironmentTeleportConfig : ScriptableObject
    {
        public Vector2 Size = new Vector2(1f, 3.2f);
    }
}