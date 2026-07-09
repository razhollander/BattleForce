using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.LevelEnvironment.Scripts
{
    public class PowerUpSpawnPoint : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
#endif
    }
}
