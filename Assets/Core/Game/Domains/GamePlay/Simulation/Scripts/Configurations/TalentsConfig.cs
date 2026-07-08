using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "TalentsConfig", menuName = "BF/Simulation/Talents Config")]
    [System.Serializable]
    public class TalentsConfig : ScriptableObject
    {
        public ushort TalentCardHealth = 2;
        [BF_SubclassList.SubclassList(typeof(BaseTalentCooldownConfig)), SerializeField]
        public TalentsCooldownsConfigs TalentsCooldownsConfigs; // this
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
        public WaterGunTalentConfig WaterGunTalentConfig;
        public HeadbuttTalentConfig HeadbuttTalentConfig;
        public FrigidBlockTalentConfig FrigidBlockTalentConfig;
        public FishingRodTalentConfig FishingRodTalentConfig;
        public SoulTalentConfig SoulTalentConfig;
        public float TalentCardWidth = 1.602175f;
        public float TalentCardHeight = 2.382844f;
    }

    [Serializable]
    public class TalentsCooldownsConfigs
    {
        [SerializeReference]
        public List<BaseTalentCooldownConfig> TalentCooldownConfigs;
    }
    
    [Serializable]
    public class TalentsArrowConfigs
    {
        public List<TalentArrowConfig> TalentCooldownConfigs;
    }
    
    [Serializable]
    public class TalentArrowConfig
    {
        public TalentType TalentType;
        public bool IsArrowShownOnlyWhilePressed;
    }
}