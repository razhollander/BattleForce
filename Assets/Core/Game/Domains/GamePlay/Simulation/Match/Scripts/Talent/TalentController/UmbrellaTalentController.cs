using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class UmbrellaTalentController : ITalentController
    {
        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly ICommandFactory _commandFactory;
        
        private ushort _casterPlayerId;
        private int _startTick;

        public TalentType TalentType => TalentType.Umbrella;
        private bool IsCurrentlyActive
        {
            get
            {
                return _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType);
            }
            set
            {
                _matchDataService.SimulationState.SetIsTalentCurrentlyActiveForPlayer(_casterPlayerId, TalentType, value);
            }
        }

        public UmbrellaTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, ICommandFactory commandFactory)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _networkConfig = networkConfig;
            _commandFactory = commandFactory;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (!wasTalentInputDownThisTick)
            {
                return;
            }

            if (IsCurrentlyActive)
            {
                DeactivateTalent(tick);
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isOnCooldown = casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown();

            if (isOnCooldown)
            {
                return;
            }

            IsCurrentlyActive = true;
            _startTick = tick;

            _netEventsDataService.AddActivateUmbrellaTalentNetEvent(tick, _casterPlayerId);
        }
        
        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            DeactivateTalent(tick);
        }

        public void OnTick(int tick, float deltaTime)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isSpinned = casterPlayerState.Spaceship.IsSpinned;
            var elapsedSeconds = (tick - _startTick) * deltaTime;
            var didTimeEnded = elapsedSeconds >= _gamePlayConfig.Talents.UmbrellaTalentConfig.DurationInSeconds;

            if (isSpinned || didTimeEnded)
            {
                DeactivateTalent(tick);

                return;
            }

            var aimDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
            _commandFactory.CreateCommandVoid<AddForceToPlayerCommand>().SetPlayerId(_casterPlayerId).SetForce(aimDirection * _gamePlayConfig.Talents.UmbrellaTalentConfig.VelocityGainPerTick * deltaTime).ShouldTurnOffEngine(false).Execute();
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Umbrella, out int talentIndex))
            {
                LogService.LogError($"No Umbrella talent found for player id {_casterPlayerId}");
                return;
            }

            ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
            talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;

            _netEventsDataService.AddDeactivateUmbrellaTalentNetEvent(tick, _casterPlayerId, cooldownEndTick);
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
