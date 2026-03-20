
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public interface ITalentController
    {
        TalentType TalentType { get; }
        bool IsCurrentlyActive { get; }
        void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime);
        void Stop();
        void OnTick(int tick);
    }
}