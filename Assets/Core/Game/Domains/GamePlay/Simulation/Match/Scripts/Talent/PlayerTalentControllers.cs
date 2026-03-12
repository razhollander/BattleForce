using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent
{
    public class PlayerTalentControllers
    {
        private SwapTalentController SwapTalentController;
        private HammerTalentController HammerTalentController;
        private BombTalentController BombTalentController;
        private SentryGunTalentController SentryGunTalentController;
        
        private ushort _casterPlayerId;

        public PlayerTalentControllers(INetEventsDataService iNetEventsDataService, IMatchDataService matchDataService)
        {
            SwapTalentController = new SwapTalentController(iNetEventsDataService, matchDataService);
            //HammerTalentController = new HammerTalentController(matchNetEventsDataService, matchDataService);
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
            SwapTalentController.SetCasterId(casterPlayerId);
        }

        private ITalentController GetTalentByType(TalentType talentType)
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

        public void ProcessTalentInput(TalentType talentType, bool isTalentInputPressed, int tick, float deltaTime)
        {
            GetTalentByType(talentType).ProcessTalentInput(isTalentInputPressed, tick, deltaTime);
        }
        
        public void OnTick(TalentType talentType, int tick)
        {
            GetTalentByType(talentType).OnTick(tick);
        }

        public void StopTalent(TalentType talentType)
        {
            GetTalentByType(talentType).Stop();
        }
    }
}