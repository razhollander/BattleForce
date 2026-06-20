using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public class SonicSlapPowerUpController : IPowerUpController
    {
        private readonly IMatchDataService _matchDataService;
        private ushort _casterPlayerId;

        public PowerUpType PowerUpType => PowerUpType.SonicSlap;

        public SonicSlapPowerUpController(IMatchDataService matchDataService)
        {
            _matchDataService = matchDataService;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void Perform(int tick)
        {
            var casterTeamId = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId).TeamId;

            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var isEnemyPlayer = playerState.TeamId != casterTeamId;
                if (!isEnemyPlayer)
                {
                    continue;
                }

                playerState.Spaceship.Transform.Direction = -playerState.Spaceship.Transform.Direction;
                playerState.Spaceship.Transform.Velocity = -playerState.Spaceship.Transform.Velocity;
            }
        }
    }
}
