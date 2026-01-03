using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations.Talents;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "TalentsConfig", menuName = "BF/Network/Talents Config")]
    public class TalentsConfig : ScriptableObject
    {
        public int MaxConcurrentTalentsForPlayer = 3;
        public HammerTalentConfig HammerTalentConfig;
        public SwapTalentConfig SwapTalentConfig;
        public PulseDashConfig PulseDashConfig;
    }
}