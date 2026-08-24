using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.LevelEnvironment.Scripts
{
    // Authoring marker for a GatePass ScoreGateObstacle. Place one in a layout, set its Id, then bake it into the
    // EnvironmentConfig via EnvironmentGenerator.RefreshScoreGates. The transform's Z euler is the gate's rotation.
    public class ScoreGateSpawnPoint : MonoBehaviour
    {
        public ushort Id;

#if UNITY_EDITOR
        [SerializeField] private Vector2 _postSize = new Vector2(1.5f, 1.5f);
        [SerializeField] private float _gapWidth = 4f;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            var right = transform.right;
            var postOffset = _gapWidth * 0.5f + _postSize.x * 0.5f;
            var postSize3D = new Vector3(_postSize.x, _postSize.y, 0.1f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(new Vector3(-postOffset, 0f, 0f), postSize3D);
            Gizmos.DrawWireCube(new Vector3(postOffset, 0f, 0f), postSize3D);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(-_gapWidth * 0.5f, 0f, 0f), new Vector3(_gapWidth * 0.5f, 0f, 0f));
        }
#endif
    }
}
