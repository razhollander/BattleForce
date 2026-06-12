using Core.Scripts.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [System.Serializable]
    public class EnvironmentSpikesConfig
    {
        public ushort Damage = 1;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 Size = new Vector2(2f, 1f);
    }
}
