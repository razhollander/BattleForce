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
        private readonly TrySpinPlayerCommand _trySpinPlayerCommand;
        private readonly TryAddForceToPlayerCommand _tryAddForceToPlayerCommand;
        private readonly PushScoreGateCommand _pushScoreGateCommand;
        private ushort _casterPlayerId;

        public PowerUpType PowerUpType => PowerUpType.Nuke;

        public NukePowerUpController(IMatchDataService matchDataService, INetEventsDataService netEventsDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, ICommandFactory commandFactory)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _trySpinPlayerCommand = commandFactory.CreateCommandVoid<TrySpinPlayerCommand>();
            _tryAddForceToPlayerCommand = commandFactory.CreateCommandVoid<TryAddForceToPlayerCommand>();
            _pushScoreGateCommand = commandFactory.CreateCommandVoid<PushScoreGateCommand>();
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void OnTick(int tick) { }

        public void Reset() { }

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

                var spinMagnitude = RNG.NextFloat(minSpin, maxSpin);
                var spinSign = RNG.NextBool() ? 1f : -1f;
                var signedSpin = spinMagnitude * spinSign;
                _trySpinPlayerCommand.SetPlayer(playerState.Id).SetSpinAmount(signedSpin).SetTick(tick).Execute();
                
                var dir = playerState.Spaceship.Transform.Position - casterPosition;
                var isAtSamePosition = dir.LengthSquared() == 0f;
                var pushDirection = isAtSamePosition ? RNG.NextFloat(0f, 360f).AngleToVector() : dir.NormalizeSafe();
                _tryAddForceToPlayerCommand.SetPlayerId(playerState.Id).SetForce(pushDirection * nukeForce).ShouldTurnOffEngine(false).Execute();
            }

            var gatePassConfig = _gamePlayConfigService.GamePlayConfig.GatePass;
            PushScoreGatesAwayFromCaster(casterPosition, gatePassConfig.NukePushImpulse, gatePassConfig.NukeSpinImpulse);

            _netEventsDataService.AddActivateNukePowerUpNetEvent(tick, _casterPlayerId, casterPosition);
        }

        // On a GatePass stage the nuke shoves every score gate too, mirroring how it pushes enemy players: away from the
        // caster with a random spin. No-op on any stage that authored no gates.
        private void PushScoreGatesAwayFromCaster(Vector2 casterPosition, float pushImpulse, float spinImpulse)
        {
            foreach (var scoreGate in _matchDataService.SimulationState.ScoreGates.AsSpan())
            {
                var dir = scoreGate.Position - casterPosition;
                var isAtSamePosition = dir.LengthSquared() == 0f;
                var pushDirection = isAtSamePosition ? RNG.NextFloat(0f, 360f).AngleToVector() : dir.NormalizeSafe();
                var spinSign = RNG.NextBool() ? 1f : -1f;

                _pushScoreGateCommand
                    .SetScoreGateId(scoreGate.Id)
                    .SetImpulse(pushDirection * pushImpulse)
                    .SetWorldContactPoint(scoreGate.Position)
                    .SetExtraSpinImpulse(spinImpulse * spinSign)
                    .Execute();
            }
        }
    }
}
