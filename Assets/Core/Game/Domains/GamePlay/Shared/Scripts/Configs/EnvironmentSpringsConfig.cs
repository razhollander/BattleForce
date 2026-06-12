using Core.Scripts.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [System.Serializable]
    public class EnvironmentSpringsConfig
    {
        public float Force = 40f;
        public float MaxSpin = 65f;
        public float MinSpin = 50f;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Size = new Vector2(2f, 1f);
    }
}
