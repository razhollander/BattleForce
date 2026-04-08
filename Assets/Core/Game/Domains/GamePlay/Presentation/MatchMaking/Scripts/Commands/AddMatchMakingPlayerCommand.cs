using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands
{
    public class AddMatchMakingPlayerCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchMakingDataService;
        private IMatchMakingPlayerControllers _playerControllers;
        private MatchMakingPlayerStateS2C _playerState;

        public AddMatchMakingPlayerCommand SetPlayerState(MatchMakingPlayerStateS2C playerState)
        {
            _playerState = playerState;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
        }

        public void Execute()
        {
            var playerId = _playerState.Id;
            LogService.LogError($"Add player: {_playerState.ToJson()}");
            _matchMakingDataService.AddPlayer(_playerState);
            _playerControllers.AddPlayer(playerId);
        }
    }
}
