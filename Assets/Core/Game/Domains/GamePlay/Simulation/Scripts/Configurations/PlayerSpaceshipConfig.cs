using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "PlayerSpaceshipConfig", menuName = "BF/Network/Player Spaceship Config")]
    public class PlayerSpaceshipConfig : ScriptableObject
    {
        public ushort StartHealth = 5;
        public float TargetMovementSpeed = 5f;
        public float RotationSpeed = 5f;
        public float ShootCooldown = 0.7f;
        public float DefaultPlayerRadius = 0.7f;
        public float EngineAcceleration = 1f;
        public float VelocityDampingPerSecond = 1f;
        public float SpinDampingPerSecond = 1f;
        public float MinSpin = 0.01f;
        public float IdleMovementSpeed = 0.25f;
    }
}