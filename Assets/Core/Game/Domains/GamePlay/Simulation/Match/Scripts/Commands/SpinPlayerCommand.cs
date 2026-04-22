using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class SpinPlayerCommand : BaseCommand, ICommandVoid
    {
        private INetEventsDataService _netEventsDataService;
        private IMatchDataService _matchDataService;
        
        private ushort _playerId;
        private float _spinAmount;
        private int _tick;

        public SpinPlayerCommand SetPlayer(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public SpinPlayerCommand SetSpinAmount(float spinAmount)
        {
            _spinAmount = spinAmount;
            return this;
        }

        public SpinPlayerCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var isRockActive = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_playerId, TalentType.Rock);
            if (isRockActive)
            {
                return;
            }
            var playerSpaceship = _matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship;
            playerSpaceship.Transform.AngularVelocity += _spinAmount;
            var isSpinningNow = playerSpaceship.Transform.AngularVelocity != 0;

            var isPlayerAlreadySpinned = playerSpaceship.IsSpinned;
            var didPlayerStartSpinning = !isPlayerAlreadySpinned && isSpinningNow;
            if (!didPlayerStartSpinning)
            {
                return;
            }

            playerSpaceship.IsSpinned = true;
            _netEventsDataService.AddPlayerSpinnedStartedNetEvent(_tick, _playerId);
        }
    }
}
