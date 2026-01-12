using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations.Talents;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "TalentsConfig", menuName = "BF/Network/Talents Config")]
    public class TalentsConfig : ScriptableObject
    {
        public int TalentCardHealth = 2;
        public SerializableDictionary<TalentType, float> CooldownPerTalentType;
        public HammerTalentConfig HammerTalentConfig;
        public SwapTalentConfig SwapTalentConfig;
        public PulseDashConfig PulseDashConfig;
        public float TalentCardWidth = 1.602175f;
        public float TalentCardHeight = 2.382844f;
    }
}