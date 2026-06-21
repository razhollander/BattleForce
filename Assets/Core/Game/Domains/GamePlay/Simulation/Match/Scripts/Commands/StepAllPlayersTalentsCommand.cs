using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepAllPlayersTalentsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPlayersTalentsManager _playersTalentsManager;
        private IPlayersPowerUpsManager _playersPowerUpsManager;

        private int _tick;
        private float _deltaTime;

        public StepAllPlayersTalentsCommand SetStepTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public StepAllPlayersTalentsCommand SetStepDeltaTime(float deltaTime)
        {
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _playersPowerUpsManager = _diContainer.Resolve<IPlayersPowerUpsManager>();
        }

        public void Execute()
        {
            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out _))
                {
                    continue;
                }

                var playerId = playerState.Id;
                if (_playersPowerUpsManager.IsPlayerAimingPowerUp(playerId))
                {
                    continue;
                }

                _playersTalentsManager.ProcessAllTalentsTickOfPlayer(playerId, _tick, _deltaTime);
            }
        }
    }
}
