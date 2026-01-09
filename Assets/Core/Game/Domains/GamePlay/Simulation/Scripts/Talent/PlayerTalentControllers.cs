using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Talent.TalentController;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Talent.TalentHandler;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Talent
{
    public class PlayerTalentControllers
    {
        public SwapTalentController SwapTalentController;
        public HammerTalentController HammerTalentController;
        public BombTalentController BombTalentController;
        public SentryGunTalentController SentryGunTalentController;

        public PlayerTalentControllers(IMatchNetEventsDataService matchNetEventsDataService, IMatchDataService matchDataService)
        {
            SwapTalentController = new SwapTalentController(matchNetEventsDataService, matchDataService);
            //HammerTalentController = new HammerTalentController(matchNetEventsDataService, matchDataService);
        }

        public ITalentController GetTalentByType(TalentType talentType)
        {
            switch (talentType)
            {
                case TalentType.Swap: return SwapTalentController;
                case TalentType.Hammer: return HammerTalentController;
                case TalentType.Bomb: return BombTalentController;
                case TalentType.SentryGun: return SentryGunTalentController;
                default: return default;
            }
        }
        
        public bool IsTalentCurrentlyActive(TalentType talentType)
        {
            switch (talentType)
            {
                case TalentType.Swap: return SwapTalentController.IsCurrentlyActive;
                case TalentType.Hammer: return HammerTalentController.IsCurrentlyActive;
                case TalentType.Bomb: return BombTalentController.IsCurrentlyActive;
                case TalentType.SentryGun: return SentryGunTalentController.IsCurrentlyActive;
                default: return false;
            }
        }
    }
}