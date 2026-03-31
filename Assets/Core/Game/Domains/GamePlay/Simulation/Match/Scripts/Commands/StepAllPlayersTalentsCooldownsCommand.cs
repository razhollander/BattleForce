using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepAllPlayersTalentsCooldownsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IOverrideableNetEventsService _overrideableNetEventsService;
        
        private int _tick;
        private float _deltaTime;

        public StepAllPlayersTalentsCooldownsCommand SetStepTick(int tick)
        {
            _tick = tick;
            return this;
        }
        
        public StepAllPlayersTalentsCooldownsCommand SetStepDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _overrideableNetEventsService = _diContainer.Resolve<IOverrideableNetEventsService>();
        }

        public void Execute()
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

    }
}