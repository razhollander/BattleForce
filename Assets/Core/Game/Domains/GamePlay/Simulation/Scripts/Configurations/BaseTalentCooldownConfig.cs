using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public abstract class BaseTalentCooldownConfig/* : ScriptableObject*/
    {
        public TalentType TalentType;
        public abstract TalentCooldownType CooldownType { get; }
    }
}