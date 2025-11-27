using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "PlayerBulletConfig", menuName = "BF/Network/Player Bullet Config")]
    public class PlayerBulletConfig : ScriptableObject
    {
        public float MoveSpeed = 10;
    }
}