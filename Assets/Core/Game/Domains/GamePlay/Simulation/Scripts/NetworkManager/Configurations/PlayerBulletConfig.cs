using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "PlayerBulletConfig", menuName = "BF/Network/Player Bullet Config")]
    public class PlayerBulletConfig : ScriptableObject
    {
        public float MoveSpeed = 10;
        public float Radius = 0.14154f;
        public ushort HitDamage = 1;
    }
}