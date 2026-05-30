using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryDamagePlayersOnTargetCommand : BaseCommand, ICommandVoid
    {
        private ICommandFactory _commandFactory;
        private ILockOnTargetTimerService _lockOnTargetTimerService;
        private INetEventsDataService _netEventsDataService;

        private int _processedTick;
        private PlayerHitCommand _playerHitCommand;

        public TryDamagePlayersOnTargetCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _lockOnTargetTimerService = _diContainer.Resolve<ILockOnTargetTimerService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _playerHitCommand = _commandFactory.CreateCommandVoid<PlayerHitCommand>();
        }

        public void Execute()
        {
            var playersToDamage = _lockOnTargetTimerService.GetPlayersToDamage();

            foreach (var pair in playersToDamage)
            {
                var casterId = pair.CasterId;
                var targetId = pair.TargetId;

                _lockOnTargetTimerService.ResetTimer(casterId, targetId);
                LogService.LogError("hitting player: " + targetId + " by player: " + casterId + "");
                _playerHitCommand
                    .SetPlayerIdGotHit(targetId)
                    .SetWasHitByAnotherPlayer(true, casterId)
                    .SetProcessedTick(_processedTick)
                    .SetHitDamage(1)
                    .Execute();

                _netEventsDataService.AddPlayerLockedOnTargetHitNetEvent(_processedTick, casterId, targetId);
            }
        }
    }
}
