using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class DashPulseTalentController : ITalentController
    {
        private readonly ICommandFactory _commandFactory;
        private ushort _casterPlayerId;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IOverrideableNetEventsService _overrideableNetEventsService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;

        public DashPulseTalentController(INetEventsDataService netEventsDataService, IOverrideableNetEventsService overrideableNetEventsService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _overrideableNetEventsService = overrideableNetEventsService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _commandFactory = commandFactory;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public TalentType TalentType => TalentType.DashPulse;
        public bool IsCurrentlyActive => false;

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (!wasTalentInputDownThisTick)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType, out int talentIndex))
            {
                return;
            }

            ref var dashPulseTalentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var doesHaveAvailableStock = dashPulseTalentModel.StocksCooldown.CurrentStocksAmount > 0;
            if (!doesHaveAvailableStock)
            {
                return;
            }

            var wasAtMaxStocks = dashPulseTalentModel.StocksCooldown.CurrentStocksAmount == dashPulseTalentModel.StocksCooldown.MaxStocksAmount;

            var remainingStocksAmount = --dashPulseTalentModel.StocksCooldown.CurrentStocksAmount;

            var direction = casterPlayerState.Spaceship.Transform.Direction;
            var pushForce = direction * _gamePlayConfig.Talents.PulseDashConfig.DashVelocity;
            _commandFactory.CreateCommandVoid<AddForceToPlayerCommand>().SetPlayerId(_casterPlayerId).SetForce(pushForce).ShouldTurnOffEngine(false).Execute();

            _netEventsDataService.AddPerformDashPulseNetEvent(tick, _casterPlayerId);
            
            if (wasAtMaxStocks)
            {
                dashPulseTalentModel.StocksCooldown.RecieveNextStockOnTick = TickUtils.GetTickPassedAfterDuration(tick, dashPulseTalentModel.StocksCooldown.MaxSingleStockCooldown, deltaTime);
            }
            
            _overrideableNetEventsService.OverrideUpdateTalentStockEvent(tick, _casterPlayerId, TalentType, remainingStocksAmount, dashPulseTalentModel.StocksCooldown.RecieveNextStockOnTick);
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
