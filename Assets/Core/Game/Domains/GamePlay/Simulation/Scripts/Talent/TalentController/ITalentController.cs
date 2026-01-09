
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Talent.TalentController
{
    public interface ITalentController
    {
        TalentType TalentType { get; }
        bool IsCurrentlyActive { get; }
        void OnTick(bool isTalentInputPressed, int tick);
    }
}