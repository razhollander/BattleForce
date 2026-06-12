using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Helpers.JsonConverters;
using Newtonsoft.Json;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    [JsonConverter(typeof(TalentCooldownConfigConverter))]
    public abstract class BaseTalentCooldownConfig
    {
        public TalentType TalentType;
        public abstract TalentCooldownType CooldownType { get; }
    }
}