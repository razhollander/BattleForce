
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public interface ITalentController
    {
        TalentType TalentType { get; }
        void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, int tick, float deltaTime);
        void StopIfActive(int tick);
        void OnTick(int tick,  float deltaTime);
        void ResetData();
    }
}