using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands
{
    public class SyncMatchMakingSimulationStateCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchDataService;
        private IMatchMakingBulletControllers _bulletControllers;
        private AddMatchMakingPlayerCommand _addMatchMakingPlayerCommand;
        private ICommandFactory _commandFactory;
        private IStartMatchButtonController _startMatchButtonController;
        
        private MatchMakingSimulationStateS2C _simulationState;
        private int _stateOccuredOnTick;

        public SyncMatchMakingSimulationStateCommand SetSimulationState(MatchMakingSimulationStateS2C simulationState)
        {
            _simulationState = simulationState;
            return this;
        }

        public SyncMatchMakingSimulationStateCommand SetStateOccuredOnTick(int stateOccuredOnTick)
        {
            _stateOccuredOnTick = stateOccuredOnTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _bulletControllers = _diContainer.Resolve<IMatchMakingBulletControllers>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _startMatchButtonController = _diContainer.Resolve<IStartMatchButtonController>();
            _addMatchMakingPlayerCommand = _commandFactory.CreateCommandVoid<AddMatchMakingPlayerCommand>();
        }

        public void Execute()
        {
            CreatePlayers();
            CreateBullets();
            _startMatchButtonController.SetIsEnabled(_simulationState.StartMatchWall.IsEnabled);
        }

        private void CreatePlayers()
        {
            foreach (var playerState in _simulationState.Players.AsSpan())
            {
                _addMatchMakingPlayerCommand.SetPlayerState(playerState).Execute();
            }
        }

        private void CreateBullets()
        {
            foreach (var bulletState in _simulationState.Bullets.AsSpan())
            {
                _matchDataService.AddBullet(bulletState.Id, bulletState.BelongToPlayerId, bulletState.Position, bulletState.Velocity, bulletState.Radius, _stateOccuredOnTick);
                _bulletControllers.CreateBullet(bulletState.Id, bulletState.Radius, bulletState.Position, bulletState.BelongToPlayerId);
            }
        }
    }
}