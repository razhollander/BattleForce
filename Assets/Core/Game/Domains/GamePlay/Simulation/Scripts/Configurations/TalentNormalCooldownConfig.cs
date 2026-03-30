using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class TalentNormalCooldownConfig : BaseTalentCooldownConfig
    {
        public override TalentCooldownType CooldownType => TalentCooldownType.Normal; 
        public float CooldownInSeconds;
    }
}