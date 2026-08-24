using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
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
        private readonly WaterGunTalentController _waterGunTalentController;
        private readonly HeadbuttTalentController _headbuttTalentController;
        private readonly RockTalentController _rockTalentController;
        private readonly FrigidBlockTalentController _frigidBlockTalentController;
        private readonly FishingRodTalentController _fishingRodTalentController;
        private readonly SoulTalentController _soulTalentController;
        private readonly FrozenTalentController _frozenTalentController;

        private ushort _casterPlayerId;
        private bool _isInitialized = false;
        public PlayerTalentControllers(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, ISimulationGamePlayConfigService gamePlayConfigService,
            IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig, IOverrideableNetEventsService overrideableNetEventsService, ICommandFactory commandFactory, SharedGamePlayConfig sharedGamePlayConfig,
            IPlayersMouseDataService playersMouseDataService, IPlayersInLavaTrackerService playersInLavaTrackerService,
            IPlayersPassedScoreGateTrackerService playersPassedScoreGateTrackerService)
        {
            _swapTalentController = new SwapTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, playersPassedScoreGateTrackerService);
            _koTalentController = new KOTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, commandFactory);
            _dashPulseTalentController = new DashPulseTalentController(netEventsDataService, overrideableNetEventsService, matchDataService, gamePlayConfigService, commandFactory);
            _sentryGunTalentController = new SentryGunTalentController(netEventsDataService, overrideableNetEventsService, matchDataService, gamePlayConfigService, networkConfig, commandFactory);
            _grapplingHookTalentController = new GrapplingHookTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, commandFactory);
            _umbrellaTalentController = new UmbrellaTalentController(netEventsDataService, matchDataService, gamePlayConfigService, networkConfig, commandFactory);
            _magneticPullTalentController = new MagneticPullTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, commandFactory);
            _chickenTalentController = new ChickenTalentController(matchDataService, netEventsDataService, gamePlayConfigService, networkConfig, physicsSimulator, commandFactory);
            _yearsOfPainTalentController = new YearsOfPainTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, commandFactory);
            _waterGunTalentController = new WaterGunTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, commandFactory);
            _headbuttTalentController = new HeadbuttTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, commandFactory);
            _rockTalentController = new RockTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, commandFactory, playersInLavaTrackerService);
            _frigidBlockTalentController = new FrigidBlockTalentController(matchDataService, gamePlayConfigService, networkConfig, sharedGamePlayConfig, commandFactory);
            _fishingRodTalentController = new FishingRodTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, commandFactory, playersMouseDataService);
            _soulTalentController = new SoulTalentController(netEventsDataService, matchDataService, gamePlayConfigService, physicsSimulator, networkConfig, sharedGamePlayConfig, playersPassedScoreGateTrackerService);
            _frozenTalentController = new FrozenTalentController(netEventsDataService, matchDataService, gamePlayConfigService, networkConfig, commandFactory, playersInLavaTrackerService);
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
            _waterGunTalentController.InitEntryPoint();
            _headbuttTalentController.InitEntryPoint();
            _rockTalentController.InitEntryPoint();
            _frigidBlockTalentController.InitEntryPoint();
            _fishingRodTalentController.InitEntryPoint();
            _frozenTalentController.InitEntryPoint();
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
            _waterGunTalentController.SetCasterId(casterPlayerId);
            _headbuttTalentController.SetCasterId(casterPlayerId);
            _rockTalentController.SetCasterId(casterPlayerId);
            _frigidBlockTalentController.SetCasterId(casterPlayerId);
            _fishingRodTalentController.SetCasterId(casterPlayerId);
            _soulTalentController.SetCasterId(casterPlayerId);
            _frozenTalentController.SetCasterId(casterPlayerId);
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
                case TalentType.WaterGun: return _waterGunTalentController;
                case TalentType.Headbutt: return _headbuttTalentController;
                case TalentType.Rock: return _rockTalentController;
                case TalentType.FrigidBlock: return _frigidBlockTalentController;
                case TalentType.FishingRod: return _fishingRodTalentController;
                case TalentType.Soul: return _soulTalentController;
                case TalentType.Frozen: return _frozenTalentController;
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
            _waterGunTalentController?.OnTick(tick, deltaTime);
            _headbuttTalentController?.OnTick(tick, deltaTime);
            _rockTalentController?.OnTick(tick, deltaTime);
            _frigidBlockTalentController?.OnTick(tick, deltaTime);
            _fishingRodTalentController?.OnTick(tick, deltaTime);
            _soulTalentController?.OnTick(tick, deltaTime);
            _frozenTalentController?.OnTick(tick, deltaTime);
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

        public void HitKOTalentWithMole(ushort moleId, int tick)
        {
            _koTalentController.HitMole(moleId, tick);
        }

        public void HitGrapplingHook(GrapplingHookHitType hitType, ushort attachedEntityId, int tick)
        {
            _grapplingHookTalentController.Hit(hitType, attachedEntityId, tick);
        }

        public void CatchFishingRodEnemy(ushort enemyPlayerId, int tick)
        {
            _fishingRodTalentController.CatchEnemy(enemyPlayerId, tick);
        }

        public void CatchFishingRodMole(ushort moleId, int tick)
        {
            _fishingRodTalentController.CatchMole(moleId, tick);
        }

        public void HitFishingRodWithWall(int tick)
        {
            _fishingRodTalentController.HitWall(tick);
        }

        public void HitSoulGhostWithWall(int tick)
        {
            _soulTalentController.HitWall(tick);
        }

        public void TryHeadbuttHitEnemy(ushort potentialCasterId, ushort potentialEnemyId, int tick)
        {
            if (potentialCasterId != _casterPlayerId) return;
            _headbuttTalentController.HitEnemy(potentialEnemyId, tick);
        }

        public void HeadbuttHitMole()
        {
            _headbuttTalentController.HitMole();
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
            _waterGunTalentController.ResetData();
            _headbuttTalentController.ResetData();
            _rockTalentController.ResetData();
            _frigidBlockTalentController.ResetData();
            _fishingRodTalentController.ResetData();
            _soulTalentController.ResetData();
            _frozenTalentController.ResetData();
        }

        public bool IsHeadbuttCharging()
        {
            return _headbuttTalentController.IsCharging;
        }
    }
}