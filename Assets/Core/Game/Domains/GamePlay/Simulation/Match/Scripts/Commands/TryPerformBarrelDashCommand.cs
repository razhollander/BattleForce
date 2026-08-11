using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryPerformBarrelDashCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private INetEventsDataService _netEventsDataService;
        private TrySpinPlayerCommand _trySpinPlayerCommand;
        private TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;

        private ushort _playerId;
        private int _processedTick;

        public TryPerformBarrelDashCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public TryPerformBarrelDashCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            var commandFactory = _diContainer.Resolve<ICommandFactory>();
            _trySpinPlayerCommand = commandFactory.CreateCommandVoid<TrySpinPlayerCommand>();
            _tryAddForceToPlayerCommand = commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
        }

        public void Execute()
        {
            var playerSpaceship = _matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship;
            if (!_gamePlayConfigService.GamePlayConfig.PlayerSpaceship.CanBarrelDash || !CanPlayerPerformBarrelDash(playerSpaceship))
            {
                return;
            }

            var spaceshipConfig = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship;
            
            _trySpinPlayerCommand.SetPlayer(_playerId).SetSpinAmount(spaceshipConfig.BarrelDashSpinAmount).SetTick(_processedTick).Execute();
            _tryAddForceToPlayerCommand.SetPlayerId(_playerId).SetForce(playerSpaceship.AimDirection * spaceshipConfig.BarrelDashForce).ShouldTurnOffEngine(false).Execute();
            _netEventsDataService.AddPerformBarrelDashNetEvent(_processedTick, _playerId);
        }

        private bool CanPlayerPerformBarrelDash(PlayerSpaceshipStateS2C playerSpaceship)
        {
            if (playerSpaceship.IsSpinned)
            {
                return false;
            }

            var hasSelectedTalent = playerSpaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent);
            var isPlayerRock = hasSelectedTalent && selectedTalent is {IsCurrentlyActive: true, TalentType: TalentType.Rock};
            var isPlayerFrozen = hasSelectedTalent && selectedTalent is {IsCurrentlyActive: true, TalentType: TalentType.Frozen};

            return !isPlayerRock && !isPlayerFrozen;
        }
    }
}
