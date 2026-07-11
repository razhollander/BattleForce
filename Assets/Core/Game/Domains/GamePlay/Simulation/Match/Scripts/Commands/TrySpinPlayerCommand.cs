using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TrySpinPlayerCommand : BaseCommand, ICommandVoid
    {
        private INetEventsDataService _netEventsDataService;
        private IMatchDataService _matchDataService;
        private IPlayersTalentsManager _playersTalentsManager;
        
        private ushort _playerId;
        private float _spinAmount;
        private int _tick;

        public TrySpinPlayerCommand SetPlayer(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }

        public TrySpinPlayerCommand SetSpinAmount(float spinAmount)
        {
            _spinAmount = spinAmount;
            return this;
        }

        public TrySpinPlayerCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
        }

        public void Execute()
        {
            var playerSpaceship = _matchDataService.SimulationState.GetPlayerById(_playerId).Spaceship;
            playerSpaceship.Transform.AngularVelocity += _spinAmount;
            var isSpinningNow = playerSpaceship.Transform.AngularVelocity != 0;

            var isPlayerAlreadySpinned = playerSpaceship.IsSpinned;
            var didPlayerStartSpinning = !isPlayerAlreadySpinned && isSpinningNow;

            if (playerSpaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent) && selectedTalent.IsCurrentlyActive &&
                selectedTalent.TalentType != TalentType.Chicken && selectedTalent.TalentType != TalentType.Rock)
            {
                _playersTalentsManager.StopTalentIfActive(selectedTalent.TalentType, _playerId, _tick);
            }
            
            if (!didPlayerStartSpinning)
            {
                return;
            }

            playerSpaceship.IsSpinned = true;
            _netEventsDataService.AddPlayerSpinnedStartedNetEvent(_tick, _playerId);
        }
    }
}
