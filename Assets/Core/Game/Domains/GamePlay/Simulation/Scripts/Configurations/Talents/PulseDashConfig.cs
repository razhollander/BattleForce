using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations.Talents
{
    [System.Serializable]
    public class PulseDashConfig
    {
        public float DashVelocity = 10f;
        public float PulsePower = 10f;
        [SerializeField] private float _pulseRectWidth = 5;
        [SerializeField] private float _pulseRectHeight = 10f;
        public Vector2 PulseRectSize => new Vector2(_pulseRectWidth, _pulseRectHeight);
    }
}