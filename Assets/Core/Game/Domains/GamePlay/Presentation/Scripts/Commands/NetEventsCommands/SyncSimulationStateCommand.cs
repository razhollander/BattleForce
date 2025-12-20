using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class SyncSimulationStateCommand : BaseCommand, ICommandVoid
    {
        private SimulationStateS2C _simulationState;
        private IMatchDataService _matchDataService;
        private IPlayerControllers _playerControllers;
        private IBulletControllers _bulletControllers;

        public SyncSimulationStateCommand SetSimulationState(SimulationStateS2C simulationState)
        {
            _simulationState = simulationState;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IBulletControllers>();
        }

        public void Execute()
        {
            CreatePlayers();
            CreateBullets();
        }

        private void CreatePlayers()
        {
            for (int i = 0; i < _simulationState.PlayersCount; i++)
            {
                var playerState = _simulationState.Players[i];
                var playerModel = _matchDataService.AddPlayer(playerState);
                _playerControllers.CreatePlayer(playerModel.PlayerId);
            }
        }

        private void CreateBullets()
        {
            if (_simulationState.Bullets.UsedCount == 0)
            {
                return;
            }
            
            foreach (var index in _simulationState.Bullets.UsedIndices())
            {
                var bulletState = _simulationState.Bullets[index];
                _matchDataService.AddBullet(bulletState.Id, bulletState.BelongToPlayerId,
                    bulletState.Position, bulletState.Radius);
                _bulletControllers.CreateBullet(bulletState.Id);
            }
        }
    }
}