using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "PlayerSpaceshipConfig", menuName = "BF/Network/Player Spaceship Config")]
    public class PlayerSpaceshipConfig : ScriptableObject
    {
        public int StartHealth = 5;
        public float MovementSpeed = 5f;
        public float RotationSpeed = 5f;
        public float ShootCooldown = 0.7f;
    }
}