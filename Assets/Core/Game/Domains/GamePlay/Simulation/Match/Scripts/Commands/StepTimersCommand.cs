using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUpsSpawner;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepTimersCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPowerUpsSpawnerService _powerUpsSpawnerService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private IHeadLessQuitterController _headLessQuitterController;
        private IStageDataService _stageDataService;
        private IOverrideableNetEventsService _overrideableNetEventsService;
        private IPreparationPhaseTimerService _preparationPhaseTimerService;
        
        private float _deltaTime;
        private int _tick;

        public StepTimersCommand SetStepTick(int tick)
        {
            _tick = tick;
            return this;
        }
        
        public StepTimersCommand SetStepDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _powerUpsSpawnerService = _diContainer.Resolve<IPowerUpsSpawnerService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _headLessQuitterController = _diContainer.Resolve<IHeadLessQuitterController>();
            _preparationPhaseTimerService = _diContainer.Resolve<IPreparationPhaseTimerService>();
            _overrideableNetEventsService = _diContainer.Resolve<IOverrideableNetEventsService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
        }

        public void Execute()
        {
            StepPlayersShootCooldown(_deltaTime);
            StepTalentsCooldowns();
            _powerUpsSpawnerService.StepTimer(_deltaTime);
            _playersInLavaTrackerService.StepTimePassedSinceLastDamageTaken(_deltaTime);
            _headLessQuitterController.StepTimer(_deltaTime);
            StepPreperationPhaseTimer(_deltaTime);
        }

        private void StepPreperationPhaseTimer(float deltaTime)
        {
            if (!_matchDataService.SimulationState.IsInPreparationPhase)
            {
                return;
            }
            
            _preparationPhaseTimerService.StepPreperationPhaseTimer(deltaTime);
        }

        private void StepTalentsCooldowns()
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                for (int i = 0; i < playerState.Spaceship.TalentsState.Talents.Count; i++)
                {
                    var playerTalent = playerState.Spaceship.TalentsState.Talents[i];

                    switch (playerTalent.CooldownType)
                    {
                        case TalentCooldownType.Stocks: TryGrantStockToPlayerTalent(playerTalent, playerState, i); break;
                        case TalentCooldownType.Normal: ClearPlayersTalentsNormalCooldownIfEnded(playerTalent, playerState, i); break;
                    }
                }
            }
        }

        private void TryGrantStockToPlayerTalent(TalentStateS2C playerTalent, PlayerStateS2C playerState, int talentIndex)
        {
            if (playerTalent.StocksCooldown.IsAtMaxStocks())
            {
                return;
            }

            if (playerTalent.StocksCooldown.RecieveNextStockOnTick > _tick)
            {
                return;
            }

            var currentStocksAmount =++playerTalent.StocksCooldown.CurrentStocksAmount;
            if (playerTalent.StocksCooldown.CurrentStocksAmount == playerTalent.StocksCooldown.MaxStocksAmount)
            {
                playerTalent.StocksCooldown.RecieveNextStockOnTick = 0;
            }
            else
            {
                playerTalent.StocksCooldown.RecieveNextStockOnTick = TickUtils.GetTickPassedAfterDuration(_tick, playerTalent.StocksCooldown.MaxSingleStockCooldown, _deltaTime);
            }

            _overrideableNetEventsService.OverrideUpdateTalentStockEvent(_tick, playerState.Id, playerTalent.TalentType, currentStocksAmount, playerTalent.StocksCooldown.RecieveNextStockOnTick);
            playerState.Spaceship.TalentsState.Talents[talentIndex] = playerTalent;
        }

        private void ClearPlayersTalentsNormalCooldownIfEnded(TalentStateS2C playerTalent, PlayerStateS2C playerState, int talentIndex) // todo check if can ref the player state
        {
            if (!playerTalent.IsOnCooldown())
            {
                return;
            }

            var didCooldownEnd = playerTalent.NormalCooldown.CooldownEndTick <= _tick;
            if (!didCooldownEnd)
            {
                return;
            }
            
            playerTalent.ClearCooldown();
            playerState.Spaceship.TalentsState.Talents[talentIndex] = playerTalent;
        }

        private void StepPlayersShootCooldown(float deltaTime)
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var shootState = playerState.Spaceship.Shoot;
                var isCurrentlyOnCooldown = shootState.CooldownSecondsLeft < shootState.MaxCooldown;
                if (isCurrentlyOnCooldown)
                {
                    shootState.CooldownSecondsLeft -= deltaTime;
                }
                
                if (shootState.CooldownSecondsLeft < 0)
                {
                    shootState.CooldownSecondsLeft = shootState.MaxCooldown;
                }
                
                playerState.Spaceship.Shoot = shootState;
            }
        }
    }
}