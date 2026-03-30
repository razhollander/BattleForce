using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class TalentStocksCooldownConfig : BaseTalentCooldownConfig
    {
        public override TalentCooldownType CooldownType => TalentCooldownType.Stocks; 
        public int MaxStocks;
        public float SingleStockCooldownInSeconds;
    }
}