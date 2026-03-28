using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class SentryGunTalentController : ITalentController
    {
        public TalentType TalentType => TalentType.SentryGun;
        public bool IsCurrentlyActive => true;//todo change this
        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            
        }

        public void StopIfActive(int tick)
        {

        }

        public void OnTick(int tick, float deltaTime)
        {
            
        }

        public void ResetData()
        {
            
        }
    }
}