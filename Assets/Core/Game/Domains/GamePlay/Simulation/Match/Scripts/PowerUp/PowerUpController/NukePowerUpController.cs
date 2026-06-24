using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public class NukePowerUpController : IPowerUpController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly SpinPlayerCommand _spinPlayerCommand;
        private readonly AddForceToPlayerCommand _addForceToPlayerCommand;
        private ushort _casterPlayerId;

        public PowerUpType PowerUpType => PowerUpType.Nuke;

        public NukePowerUpController(IMatchDataService matchDataService, INetEventsDataService netEventsDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _spinPlayerCommand = commandFactory.CreateCommandVoid<SpinPlayerCommand>();
            _addForceToPlayerCommand = commandFactory.CreateCommandVoid<AddForceToPlayerCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void Perform(int tick)
        {
            var casterState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var casterTeamId = casterState.TeamId;
            var casterPosition = casterState.Spaceship.Transform.Position;
            var config = _gamePlayConfigService.GamePlayConfig.PowerUps;
            var nukeForce = config.NukeForce;
            var minSpin = config.NukeMinSpinAmount;
            var maxSpin = config.NukeMaxSpinAmount;

            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                var isSameTeam = playerState.TeamId == casterTeamId;
                if (isSameTeam) continue;

                var dir = playerState.Spaceship.Transform.Position - casterPosition;
                var isAtSamePosition = dir.LengthSquared() == 0f;
                var pushDirection = isAtSamePosition ? RNG.NextFloat(0f, 360f).AngleToVector() : Vector2.Normalize(dir);
                _addForceToPlayerCommand.SetPlayerId(playerState.Id).SetForce(pushDirection * nukeForce).ShouldTurnOffEngine(false).Execute();

                var spinMagnitude = RNG.NextFloat(minSpin, maxSpin);
                var spinSign = RNG.NextBool() ? 1f : -1f;
                var signedSpin = spinMagnitude * spinSign;
                _spinPlayerCommand.SetPlayer(playerState.Id).SetSpinAmount(signedSpin).SetTick(tick).Execute();
            }

            _netEventsDataService.AddActivateNukePowerUpNetEvent(tick, _casterPlayerId, casterPosition);
        }
    }
}
