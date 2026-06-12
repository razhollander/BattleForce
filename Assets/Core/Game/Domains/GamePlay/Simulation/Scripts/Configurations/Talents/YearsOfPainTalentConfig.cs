using System;
using Core.Scripts.Helpers;
using Newtonsoft.Json;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class YearsOfPainTalentConfig
    {
        [JsonConverter(typeof(Vector2Converter))]
        public UnityEngine.Vector2 RectangleColliderSize = new UnityEngine.Vector2(3f, 1.5f);
        public float PushForce = 100;
        public float MaxSpin = 200f;
        public float MinSpin = 180;
    }
}
