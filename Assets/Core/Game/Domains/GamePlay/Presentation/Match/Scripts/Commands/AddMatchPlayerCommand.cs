using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class AddMatchPlayerCommand :BaseCommand, ICommandVoid
    {
        private IMatchPlayerControllers _playerControllers;
        private IMatchPlayerUIControllers _playerUIControllers;
        private IWorldCameraController _worldCameraController;
        private IMatchDataService _matchDataService;
        private PlayerStateS2C _playerState;
        private int _currentServerTick;

        public AddMatchPlayerCommand SetPlayerState(PlayerStateS2C playerState)
        {
            _playerState = playerState;
            return this;
        }
        
        public AddMatchPlayerCommand SetCurrentServerTick(int currentServerTick)
        {
            _currentServerTick = currentServerTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var playerId = _playerState.Id;
            _matchDataService.AddPlayer(_playerState);
            _playerControllers.AddPlayer(playerId);
            _playerUIControllers.AddPlayer(playerId, _currentServerTick);
            _worldCameraController.AddTarget(_playerControllers.GetPlayerSpaceshipTransform(playerId));
        }
    }
}