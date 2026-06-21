using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands
{
    public class AddMatchMakingPlayerCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchMakingDataService;
        private IMatchMakingPlayerControllers _playerControllers;
        private ILockOnTargetEffectController _lockOnTargetEffectController;
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
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
        }

        public void Execute()
        {
            var playerId = _playerState.Id;
            _matchMakingDataService.AddPlayer(_playerState);
            _playerControllers.AddPlayer(playerId);
            _lockOnTargetEffectController.AddPlayer(playerId, _playerState.Spaceship.ObjectsLockedOnTarget);
        }
    }
}
