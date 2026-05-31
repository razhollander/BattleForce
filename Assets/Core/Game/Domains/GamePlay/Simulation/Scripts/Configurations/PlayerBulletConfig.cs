using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class PlayerBulletConfig
    {
        public float MoveSpeed = 10;
        public float Radius = 0.14154f;
        public ushort HitDamage = 1;
    }
}