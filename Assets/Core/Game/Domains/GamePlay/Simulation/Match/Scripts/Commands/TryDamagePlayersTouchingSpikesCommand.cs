using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryDamagePlayersTouchingSpikesCommand : BaseCommand, ICommandVoid
    {
        private ICommandFactory _commandFactory;
        private IPlayersTouchingSpikesTrackerService _playersTouchingSpikesTrackerService;

        private int _processedTick;
        private CollidePlayerWithEnvironmentSpikeCommand _collidePlayerWithEnvironmentSpikeCommand;

        public TryDamagePlayersTouchingSpikesCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _playersTouchingSpikesTrackerService = _diContainer.Resolve<IPlayersTouchingSpikesTrackerService>();
            _collidePlayerWithEnvironmentSpikeCommand = _commandFactory.CreateCommandVoid<CollidePlayerWithEnvironmentSpikeCommand>();
        }

        public void Execute()
        {
            var playersToDamage = _playersTouchingSpikesTrackerService.GetPlayersToDamage();

            for (int i = 0; i < playersToDamage.Count; i++)
            {
                var playerToDamage = playersToDamage[i];
                _playersTouchingSpikesTrackerService.TryResetPlayerTimePassedSinceLastDamageTaken(playerToDamage.PlayerId);
                _collidePlayerWithEnvironmentSpikeCommand
                    .SetPlayerId(playerToDamage.PlayerId)
                    .SetSpikeId(playerToDamage.SpikeId)
                    .SetProcessedTick(_processedTick)
                    .Execute();
            }
        }
    }
}
