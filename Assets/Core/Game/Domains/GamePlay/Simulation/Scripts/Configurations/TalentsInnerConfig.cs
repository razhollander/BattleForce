using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class TalentsInnerConfig
    {
        public ushort TalentCardHealth = 2;
        [BF_SubclassList.SubclassList(typeof(BaseTalentCooldownConfig)), SerializeField]
        public TalentsCooldownsConfigs TalentsCooldownsConfigs;
        public TalentsArrowConfigs TalentsArrowConfigs;
        public HammerTalentConfig HammerTalentConfig;
        public SwapTalentConfig SwapTalentConfig;
        public PulseDashConfig PulseDashConfig;
        public KOTalentConfig KOTalentConfig;
        public SentryGunTalentConfig SentryGunTalentConfig;
        public GrapplingHookTalentConfig GrapplingHookTalentConfig;
        public UmbrellaTalentConfig UmbrellaTalentConfig;
        public MagneticPullTalentConfig MagneticPullTalentConfig;
        public ChickenTalentConfig ChickenTalentConfig;
        public YearsOfPainTalentConfig YearsOfPainTalentConfig;
        public RockTalentConfig RockTalentConfig;
        public float TalentCardWidth = 1.602175f;
        public float TalentCardHeight = 2.382844f;
    }
}