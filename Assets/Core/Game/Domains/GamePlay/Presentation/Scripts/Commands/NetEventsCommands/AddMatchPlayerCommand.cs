using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.NetEventsCommands
{
    public class AddMatchPlayerCommand :BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPlayerControllers _playerControllers;
        private IMatchPlayerUIControllers _playerUIControllers;
        private IWorldCameraController _worldCameraController;
        private PlayerStateS2C _playerState;

        public AddMatchPlayerCommand SetPlayerState(PlayerStateS2C playerState)
        {
            _playerState = playerState;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playerControllers = _diContainer.Resolve<IPlayerControllers>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
        }

        public void Execute()
        {
            LogService.LogError($"Added player {_playerState.Id}, 1");
            var playerModel = _matchDataService.AddPlayer(_playerState);
            var playerId = playerModel.PlayerId;
            _playerControllers.AddPlayer(playerId);
            _playerUIControllers.AddPlayer(playerId);
            _worldCameraController.AddTarget(_playerControllers.GetPlayerTranform(playerId));
            LogService.LogError($"Added player {_playerState.Id}, 2");
        }
    }
}