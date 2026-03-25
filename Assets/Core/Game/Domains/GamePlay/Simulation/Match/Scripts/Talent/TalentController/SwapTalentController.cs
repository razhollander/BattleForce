using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class SwapTalentController : ITalentController
    {
        private ushort _casterPlayerId;

        private readonly INetEventsDataService _netEventsDataService;
        private readonly IMatchDataService _matchDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly NetworkConfig _networkConfig;

        public TalentType TalentType => TalentType.Swap;
        public bool IsCurrentlyActive { get; private set; }
        
        private ushort _currentActiveSwapFieldId;

        public SwapTalentController(INetEventsDataService netEventsDataService, IMatchDataService matchDataService, SimulationGamePlayConfig gamePlayConfig, IPhysicsSimulator physicsSimulator, NetworkConfig networkConfig)
        {
            _netEventsDataService = netEventsDataService;
            _matchDataService = matchDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void ProcessTalentInput(bool isTalentInputPressed, int tick, float deltaTime)
        {
            if (IsCurrentlyActive || !isTalentInputPressed)
            {
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            if (casterPlayerState.Spaceship.TalentsState.GetCurrentSelectedTalent().IsOnCooldown())
            {
                return;
            }

            IsCurrentlyActive = true;
            var talentsSwapTalentConfig = _gamePlayConfig.Talents.SwapTalentConfig;
            var fieldEndTick = TickUtils.GetTickPassedAfterDuration(tick,talentsSwapTalentConfig.GrowDurationSeconds, _networkConfig.DeltaTime);
            var swapFieldModel = _matchDataService.AddSwapField(_casterPlayerId, tick, fieldEndTick);
            _currentActiveSwapFieldId = swapFieldModel.Id;
            _physicsSimulator.AddSwapField(swapFieldModel.Id, casterPlayerState.TeamId, casterPlayerState.Spaceship.Transform.Position);
            _netEventsDataService.AddCreateSwapFieldNetEvent(tick, swapFieldModel.Id, _casterPlayerId, fieldEndTick, talentsSwapTalentConfig.MaxRadius);
        }

        public void StopIfActive(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            DeactivateTalent(tick);
        }

        public void OnTick(int tick)
        {
            if (!IsCurrentlyActive)
            {
                return;
            }

            ref var swapFieldModel = ref _matchDataService.SimulationState.GetSwapFieldById(_currentActiveSwapFieldId);
            var didEnd = tick >= swapFieldModel.EndTick;

            if (didEnd)
            {
                DeactivateTalent(tick);
            }
            else
            {
                UpdateSwapFieldSize(tick, ref swapFieldModel);
            }
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
            _currentActiveSwapFieldId = 0;
        }

        private void UpdateSwapFieldSize(int tick, ref TalentSwapFieldS2C swapFieldModel)
        {
            swapFieldModel.UpdateRadiusForTick(tick, _gamePlayConfig.Talents.SwapTalentConfig.MaxRadius);
        }

        public void PerformTalentWithEnemy(ushort enemyPlayerId, int tick)
        {
            if (!IsCurrentlyActive)
            {
                LogService.LogError($"Swap talent for player {_casterPlayerId} is not active!");
                return;
            }

            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var enemyPlayerState = _matchDataService.SimulationState.GetPlayerById(enemyPlayerId);
            SwapPlayersTransform(casterPlayerState, enemyPlayerState);
            _netEventsDataService.AddPlayersSwapEvent(tick, _casterPlayerId, enemyPlayerId,
                casterPlayerState.Spaceship.Transform.Position, enemyPlayerState.Spaceship.Transform.Position,
                casterPlayerState.Spaceship.Transform.Direction, enemyPlayerState.Spaceship.Transform.Direction);
            
            DeactivateTalent(tick);
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.Swap, out int talentIndex))
            {
                LogService.LogError($"No swap talent found for player id {_casterPlayerId}");
                return;
            }
            ref var swapTalentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);

            var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, swapTalentModel.MaxCooldown, _networkConfig.DeltaTime);
            swapTalentModel.CooldownEndTick = cooldownEndTick;

            _physicsSimulator.RemoveSwapField(_currentActiveSwapFieldId);
            _matchDataService.SimulationState.RemoveSwapFieldById(_currentActiveSwapFieldId);
            _netEventsDataService.AddDeactivateSwapTalentNetEvent(tick, _casterPlayerId, _currentActiveSwapFieldId, cooldownEndTick);
        }

        private void SwapPlayersTransform(PlayerStateS2C casterPlayerState, PlayerStateS2C closetPlayerToCaster)
        {
            (casterPlayerState.Spaceship.Transform.Position, closetPlayerToCaster.Spaceship.Transform.Position) =
                (closetPlayerToCaster.Spaceship.Transform.Position, casterPlayerState.Spaceship.Transform.Position);

            (casterPlayerState.Spaceship.Transform.Direction, closetPlayerToCaster.Spaceship.Transform.Direction) =
                (closetPlayerToCaster.Spaceship.Transform.Direction, casterPlayerState.Spaceship.Transform.Direction);
            
            (casterPlayerState.Spaceship.Transform.Velocity, closetPlayerToCaster.Spaceship.Transform.Velocity) =
                (closetPlayerToCaster.Spaceship.Transform.Velocity, casterPlayerState.Spaceship.Transform.Velocity);
            
            (casterPlayerState.Spaceship.Transform.AngularVelocity, closetPlayerToCaster.Spaceship.Transform.AngularVelocity) =
                (closetPlayerToCaster.Spaceship.Transform.AngularVelocity, casterPlayerState.Spaceship.Transform.AngularVelocity);
        }
    }
}