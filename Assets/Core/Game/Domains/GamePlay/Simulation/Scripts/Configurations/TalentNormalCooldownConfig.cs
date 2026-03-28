using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    // [CreateAssetMenu(fileName = "TalentsNormalCooldownConfig", menuName = "BF/Simulation/Talents Normal Cooldown Config")]
    [System.Serializable]
    public class TalentNormalCooldownConfig : BaseTalentCooldownConfig
    {
        public override TalentCooldownType CooldownType => TalentCooldownType.Normal; 
        public float CooldownInSeconds;
    }
}