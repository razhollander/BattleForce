using System;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class YearsOfPainTalentConfig
    {
        public UnityEngine.Vector2 RectangleColliderSize = new UnityEngine.Vector2(4f, 8f);
        public float PushForce = 20f;
        public float MaxSpin = 60f;
        public float MinSpin = 40f;
    }
}
