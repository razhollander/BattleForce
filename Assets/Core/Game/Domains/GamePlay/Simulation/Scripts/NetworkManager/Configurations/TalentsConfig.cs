using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations.Talents;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "TalentsConfig", menuName = "BF/Network/Talents Config")]
    public class TalentsConfig : ScriptableObject
    {
        public int TalentCardHealth;
        public SerializableDictionary<TalentType, float> CooldownPerTalentType;
        public HammerTalentConfig HammerTalentConfig;
        public SwapTalentConfig SwapTalentConfig;
        public PulseDashConfig PulseDashConfig;
    }
}