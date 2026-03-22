using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent
{
    public class PlayerTalentControllers
    {
        private readonly SwapTalentController _swapTalentController;
        private HammerTalentController HammerTalentController;
        private BombTalentController BombTalentController;
        private SentryGunTalentController SentryGunTalentController;
        
        private ushort _casterPlayerId;

        public PlayerTalentControllers(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig)
        {
            _swapTalentController = new SwapTalentController(netEventsDataService, matchDataService, gamePlayConfig, physicsSimulator, networkConfig);
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
            _swapTalentController.SetCasterId(casterPlayerId);
        }

        private ITalentController GetTalentByType(TalentType talentType)
        {
            switch (talentType)
            {
                case TalentType.Swap: return _swapTalentController;
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
                case TalentType.Swap: return _swapTalentController.IsCurrentlyActive;
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
        
        public void OnTick(int tick)
        {
            _swapTalentController?.OnTick(tick);
            HammerTalentController?.OnTick(tick);
            BombTalentController?.OnTick(tick);
            SentryGunTalentController?.OnTick(tick);
        }

        public void StopTalentIfActive(TalentType talentType, int tick)
        {
            GetTalentByType(talentType).StopIfActive(tick);
        }

        public void CompleteSwapTalentWithEnemy(PlayerStateS2C enemyPlayer, int tick)
        {
            _swapTalentController.PerformTalentWithEnemy(enemyPlayer, tick);
        }

        public void ResetData()
        {
            _swapTalentController.ResetData();
            // HammerTalentController.ResetData();
            // BombTalentController.ResetData();
            // SentryGunTalentController.ResetData();
        }
    }
}