using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Talent.TalentController
{
    public class SentryGunTalentController : ITalentController
    {
        public TalentType TalentType => TalentType.SentryGun;
        public bool IsCurrentlyActive => true;//todo change this
        public void OnTick(bool isTalentInputPressed, int tick)
        {
            
        }
    }
}