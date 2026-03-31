using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public abstract class BaseTalentCooldownConfig
    {
        public TalentType TalentType;
        public abstract TalentCooldownType CooldownType { get; }
    }
}