using System;
using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [CreateAssetMenu(fileName = "TalentsConfig", menuName = "BF/Simulation/Talents Config")]
    public class TalentsConfig : ScriptableObject
    {
        public ushort TalentCardHealth = 2;
        [BF_SubclassList.SubclassList(typeof(BaseTalentCooldownConfig)), SerializeField]
        public TalentsCooldownsConfigs TalentsCooldownsConfigs;
        public HammerTalentConfig HammerTalentConfig;
        public SwapTalentConfig SwapTalentConfig;
        public PulseDashConfig PulseDashConfig;
        public KOTalentConfig KOTalentConfig;
        public SentryGunTalentConfig SentryGunTalentConfig;
        public GrapplingHookTalentConfig GrapplingHookTalentConfig;
        public MagneticPullTalentConfig MagneticPullTalentConfig;
        public float TalentCardWidth = 1.602175f;
        public float TalentCardHeight = 2.382844f;
    }

    [Serializable]
    public class TalentsCooldownsConfigs
    {
        [SerializeReference]
        public List<BaseTalentCooldownConfig> TalentCooldownConfigs;
    }
        
}