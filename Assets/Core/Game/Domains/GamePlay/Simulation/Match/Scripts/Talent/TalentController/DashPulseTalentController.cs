using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class DashPulseTalentController : ITalentController
    {
        private ushort _casterPlayerId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;

        public DashPulseTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public TalentType TalentType => TalentType.DashPulse;
        public bool IsCurrentlyActive { get; private set; }

        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive || !isTalentInputPressed)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.DashPulse, out int talentIndex))
            {
                return;
            }

            ref var dashPulseTalentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);

            if (dashPulseTalentModel.CurrentStocksAmount == 0)
            {
                return;
            }

            bool wasAtMaxStocks = dashPulseTalentModel.CurrentStocksAmount == dashPulseTalentModel.MaxStocksAmount;

            dashPulseTalentModel.CurrentStocksAmount--;

            var direction = casterPlayerState.Spaceship.Transform.Direction;
            var pushForce = direction * _gamePlayConfig.Talents.PulseDashConfig.DashVelocity;
            casterPlayerState.Spaceship.Transform.Velocity += pushForce;

            _netEventsDataService.AddPerformDashPulseNetEvent(tick, _casterPlayerId);

            if (wasAtMaxStocks)
            {
                dashPulseTalentModel.ReceiveStockOnTick = TickUtils.GetTickPassedAfterDuration(tick, dashPulseTalentModel.MaxCooldown, deltaTime);
            }

            if (dashPulseTalentModel.CurrentStocksAmount == 0)
            {
                var cooldownEndTick = dashPulseTalentModel.ReceiveStockOnTick;
                dashPulseTalentModel.CooldownEndTick = cooldownEndTick;
                _netEventsDataService.AddDeactivateDashPulseTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
            }
        }

        public void StopIfActive(int tick)
        {

        }

        public void OnTick(int tick)
        {

        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
