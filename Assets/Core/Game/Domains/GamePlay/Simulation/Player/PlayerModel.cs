using UnityEngine;

namespace Core.Domains.GamePlay.Player
{
    public struct PlayerModel
    {
        public int Id;
        public PlayerTransformModel TransformModel;
        public PlayerShootModel ShootModel;
        public PlayerHealthModel HealthModel;
    }

    public struct PlayerTransformModel
    {
        public Vector2 Position;
        public Quaternion Rotation;
        public Vector2 Velocity;
        public Vector2 AngularVelocity;
    }
    
    public struct PlayerHealthModel
    {
        public int MaxHealth;
        public int CurrentHealth;
    }

    public struct PlayerShootModel
    {
        public float MaxShootLoadingSeconds;
        public float ShootLoadingSecondsLeft;
        public float ShootRecoilForce;
    }
}