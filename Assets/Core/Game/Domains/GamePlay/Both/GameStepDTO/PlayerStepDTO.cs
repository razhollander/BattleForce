using UnityEngine;

namespace Core.Game.Domains.GamePlay.Both.GameStepState
{
    public struct PlayerTransformDTO
    {
        public int Id;
        // todo json serialize as shorter name
        public Vector2 CurrentPosition;
        public Quaternion CurrentRotation;
    }
    
    public struct PlayerShootDTO
    {
        public int Id;
        public float ShootLoadingSecondsLeft;
    }

    public struct PlayerHealthDTO
    {
        public int Id;
        public int CurrentHealth;
    }
    
    public struct PlayerBulletDTO
    {
        public int Id;
        public Vector2 CurrentPosition;
    }

    public struct PlayerShootBulletEventDTO
    {
        public int BulletId;
        public int PlayerId;
        public Vector3 ShootPosition;
    }
    
    public struct BulletHitPlayerEventDTO
    {
        public int BulletId;
        public int PlayerId;
    }
}