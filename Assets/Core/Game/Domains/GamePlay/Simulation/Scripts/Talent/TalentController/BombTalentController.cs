using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Talent.TalentController
{
    public class BombTalentController : ITalentController
    {
        public TalentType TalentType => TalentType.Bomb;
        public bool IsCurrentlyActive => true;//todo change this
        public void OnTick(bool isTalentInputPressed, int tick)
        {
            
        }

        public void Stop()
        {

        }
    }
}