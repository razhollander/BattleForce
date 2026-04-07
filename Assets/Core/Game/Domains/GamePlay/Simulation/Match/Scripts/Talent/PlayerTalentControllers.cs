using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent
{
    public class PlayerTalentControllers
    {
        private readonly SwapTalentController _swapTalentController;
        private readonly KOTalentController _koTalentController;
        private readonly DashPulseTalentController _dashPulseTalentController;
        private HammerTalentController HammerTalentController;
        private BombTalentController BombTalentController;
        private readonly SentryGunTalentController _sentryGunTalentController;
        
        private ushort _casterPlayerId;

        public PlayerTalentControllers(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, IOverrideableNetEventsService overrideableNetEventsService, ICommandFactory commandFactory)
        {
            _swapTalentController = new SwapTalentController(netEventsDataService, matchDataService, gamePlayConfig, physicsSimulator, networkConfig);
            _koTalentController = new KOTalentController(netEventsDataService, matchDataService, gamePlayConfig, physicsSimulator, networkConfig);
            _dashPulseTalentController = new DashPulseTalentController(netEventsDataService, overrideableNetEventsService, matchDataService, gamePlayConfig);
            _sentryGunTalentController = new SentryGunTalentController(netEventsDataService, overrideableNetEventsService, matchDataService, gamePlayConfig, networkConfig, commandFactory);
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
            _swapTalentController.SetCasterId(casterPlayerId);
            _koTalentController.SetCasterId(casterPlayerId);
            _dashPulseTalentController.SetCasterId(casterPlayerId);
            _sentryGunTalentController.SetCasterId(casterPlayerId);
        }

        private ITalentController GetTalentByType(TalentType talentType)
        {
            switch (talentType)
            {
                case TalentType.Swap: return _swapTalentController;
                case TalentType.KO: return _koTalentController;
                case TalentType.Hammer: return HammerTalentController;
                case TalentType.Bomb: return BombTalentController;
                case TalentType.SentryGun: return _sentryGunTalentController;
                case TalentType.DashPulse: return _dashPulseTalentController;
                default: return default;
            }
        }

        public void ProcessTalentInput(TalentType talentType, bool isTalentInputPressed, int tick, float deltaTime)
        {
            GetTalentByType(talentType).ProcessTalentInput(isTalentInputPressed, tick, deltaTime);
        }
        
        public void OnTick(int tick, float deltaTime)
        {
            _swapTalentController?.OnTick(tick, deltaTime);
            _koTalentController?.OnTick(tick, deltaTime);
            HammerTalentController?.OnTick(tick, deltaTime);
            BombTalentController?.OnTick(tick, deltaTime);
            _sentryGunTalentController?.OnTick(tick, deltaTime);
            _dashPulseTalentController?.OnTick(tick, deltaTime);
        }

        public void StopTalentIfActive(TalentType talentType, int tick)
        {
            GetTalentByType(talentType).StopIfActive(tick);
        }

        public void CompleteSwapTalentWithEnemy(ushort enemyPlayerId, int tick)
        {
            _swapTalentController.PerformTalentWithEnemy(enemyPlayerId, tick);
        }

        public void HitKOTalentWithEnemy(ushort enemyPlayerId, int tick)
        {
            _koTalentController.HitEnemyPlayer(enemyPlayerId, tick);
        }

        public void HitKOTalentWithWall()
        {
            _koTalentController.HitWall();
        }

        public void ResetData()
        {
            _swapTalentController.ResetData();
            _koTalentController.ResetData();
            _dashPulseTalentController.ResetData();
            // HammerTalentController.ResetData();
            // BombTalentController.ResetData();
            _sentryGunTalentController.ResetData();
        }
    }
}