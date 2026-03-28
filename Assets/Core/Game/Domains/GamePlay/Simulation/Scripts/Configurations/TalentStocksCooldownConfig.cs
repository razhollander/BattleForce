using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    // [CreateAssetMenu(fileName = "TalentsNormalCooldownConfig", menuName = "BF/Simulation/Talents Stocks Cooldown Config")]
    [System.Serializable]
    public class TalentStocksCooldownConfig : BaseTalentCooldownConfig
    {
        public override TalentCooldownType CooldownType => TalentCooldownType.Stocks; 
        public int MaxStocks;
        public float SingleStockCooldownInSeconds;
    }
}