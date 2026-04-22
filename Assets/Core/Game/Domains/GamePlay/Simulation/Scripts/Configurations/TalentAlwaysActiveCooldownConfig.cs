using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    public class TalentAlwaysActiveCooldownConfig: BaseTalentCooldownConfig
    {
        public override TalentCooldownType CooldownType => TalentCooldownType.AlwaysActive;
    }
}