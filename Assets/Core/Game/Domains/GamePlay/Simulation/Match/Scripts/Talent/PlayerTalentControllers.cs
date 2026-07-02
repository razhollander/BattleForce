using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
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
        private readonly SentryGunTalentController _sentryGunTalentController;
        private readonly GrapplingHookTalentController _grapplingHookTalentController;
        private readonly UmbrellaTalentController _umbrellaTalentController;
        private readonly MagneticPullTalentController _magneticPullTalentController;
        private readonly ChickenTalentController _chickenTalentController;
        private readonly YearsOfPainTalentController _yearsOfPainTalentController;
        
        private ushort _casterPlayerId;
        private bool _isInitialized = false;
        public PlayerTalentControllers(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, IOverrideableNetEventsService overrideableNetEventsService, ICommandFactory commandFactory, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _swapTalentController = new SwapTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig);
            _koTalentController = new KOTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, commandFactory);
            _dashPulseTalentController = new DashPulseTalentController(netEventsDataService, overrideableNetEventsService, matchDataService, gamePlayConfigService, commandFactory);
            _sentryGunTalentController = new SentryGunTalentController(netEventsDataService, overrideableNetEventsService, matchDataService, gamePlayConfigService, networkConfig, commandFactory);
            _grapplingHookTalentController = new GrapplingHookTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, commandFactory);
            _umbrellaTalentController = new UmbrellaTalentController(netEventsDataService, matchDataService, gamePlayConfigService, networkConfig, commandFactory);
            _magneticPullTalentController = new MagneticPullTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, commandFactory);
            _chickenTalentController = new ChickenTalentController(matchDataService, netEventsDataService, gamePlayConfigService, networkConfig, physicsSimulator, commandFactory);
            _yearsOfPainTalentController = new YearsOfPainTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, commandFactory);
        }

        public void InitEntryPoint()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            _koTalentController.InitEntryPoint();
            _magneticPullTalentController.InitEntryPoint();
            _yearsOfPainTalentController.InitEntryPoint();
        }
        
        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
            _swapTalentController.SetCasterId(casterPlayerId);
            _koTalentController.SetCasterId(casterPlayerId);
            _dashPulseTalentController.SetCasterId(casterPlayerId);
            _sentryGunTalentController.SetCasterId(casterPlayerId);
            _grapplingHookTalentController.SetCasterId(casterPlayerId);
            _umbrellaTalentController.SetCasterId(casterPlayerId);
            _magneticPullTalentController.SetCasterId(casterPlayerId);
            _chickenTalentController.SetCasterId(casterPlayerId);
            _yearsOfPainTalentController.SetCasterId(casterPlayerId);
        }

        private ITalentController GetTalentByType(TalentType talentType)
        {
            switch (talentType)
            {
                case TalentType.Swap: return _swapTalentController;
                case TalentType.KO: return _koTalentController;
                case TalentType.SentryGun: return _sentryGunTalentController;
                case TalentType.DashPulse: return _dashPulseTalentController;
                case TalentType.GrapplingHook: return _grapplingHookTalentController;
                case TalentType.Umbrella: return _umbrellaTalentController;
                case TalentType.MagneticPull: return _magneticPullTalentController;
                case TalentType.Chicken: return _chickenTalentController;
                case TalentType.YearsOfPain: return _yearsOfPainTalentController;
                default: return default;
            }
        }

        public void ProcessTalentInput(TalentType talentType, bool wasTalentInputDownThisTick, bool isTalentInputPressed, bool wasTalentInputReleasedThisTick, int tick, float deltaTime)
        {
            GetTalentByType(talentType).ProcessTalentInput(wasTalentInputDownThisTick, isTalentInputPressed, wasTalentInputReleasedThisTick, tick, deltaTime);
        }
        
        public void OnTick(int tick, float deltaTime)
        {
            _swapTalentController?.OnTick(tick, deltaTime);
            _koTalentController?.OnTick(tick, deltaTime);
            _sentryGunTalentController?.OnTick(tick, deltaTime);
            _dashPulseTalentController?.OnTick(tick, deltaTime);
            _grapplingHookTalentController?.OnTick(tick, deltaTime);
            _umbrellaTalentController?.OnTick(tick, deltaTime);
            _magneticPullTalentController?.OnTick(tick, deltaTime);
            _chickenTalentController?.OnTick(tick, deltaTime);
            _yearsOfPainTalentController?.OnTick(tick, deltaTime);
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

        public void HitGrapplingHookWithWall(ushort wallId, int tick)
        {
            _grapplingHookTalentController.HitWall(wallId, tick);
        }

        public void ResetData()
        {
            _swapTalentController.ResetData();
            _koTalentController.ResetData();
            _dashPulseTalentController.ResetData();
            _sentryGunTalentController.ResetData();
            _grapplingHookTalentController.ResetData();
            _umbrellaTalentController.ResetData();
            _magneticPullTalentController.ResetData();
            _chickenTalentController.ResetData();
            _yearsOfPainTalentController.ResetData();
        }
    }
}