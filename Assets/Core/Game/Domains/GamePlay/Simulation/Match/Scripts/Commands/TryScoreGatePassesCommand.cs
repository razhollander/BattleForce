using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.ScoreGate;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryScoreGatePassesCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IStageDataService _stageDataService;
        private IPlayersPassedScoreGateTrackerService _playersPassedScoreGateTrackerService;
        private INetEventsDataService _netEventsDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private NetworkConfig _networkConfig;

        private int _processedTick;

        public TryScoreGatePassesCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _playersPassedScoreGateTrackerService = _diContainer.Resolve<IPlayersPassedScoreGateTrackerService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
        }

        public void Execute()
        {
            var simulationState = _matchDataService.SimulationState;
            if (simulationState.StageType != StageType.GatePass)
            {
                return;
            }
            
            var canScoreInStagePhase = !simulationState.IsInPreparationPhase && !_stageDataService.IsStageEnded;
            var gapHalfWidth = _sharedGamePlayConfig.ScoreGateGapWidth * simulationState.MapSizeMultiplier * 0.5f;

            foreach (var player in simulationState.Players.AsSpan())
            {
                var currentPosition = player.Spaceship.Transform.Position;

                if (canScoreInStagePhase && _playersPassedScoreGateTrackerService.TryGetPlayerPreviousPosition(player.Id, out var previousPosition))
                {
                    TryScorePlayerAgainstAllGates(player, previousPosition, currentPosition, gapHalfWidth);
                }

                _playersPassedScoreGateTrackerService.SetPlayerPreviousPosition(player.Id, currentPosition);
            }
        }

        private void TryScorePlayerAgainstAllGates(PlayerStateS2C player, Vector2 previousPosition, Vector2 currentPosition, float gapHalfWidth)
        {
            // A teleport (Swap, Soul respawn, teleport gate) is an arena-scale one-tick jump; treating its segment as a
            // pass would award a free point, so any implausibly long movement is skipped this tick.
            // var teleportThreshold = _gamePlayConfigService.GamePlayConfig.GatePass.TeleportDetectionSegmentLength * _matchDataService.SimulationState.MapSizeMultiplier;
            // if (Vector2.Distance(previousPosition, currentPosition) > teleportThreshold)
            // {
            //     return;
            // }

            var scoreGates = _matchDataService.SimulationState.ScoreGates;

            for (int i = 0; i < scoreGates.Count; i++)
            {
                var scoreGate = scoreGates[i];

                if (_playersPassedScoreGateTrackerService.IsPlayerPassScoreOnCooldown(player.Id, scoreGate.Id, _processedTick))
                {
                    continue;
                }

                ScoreGateGeometryUtils.GetGapSegment(scoreGate.Position, scoreGate.Rotation, gapHalfWidth, out var gapStart, out var gapEnd);

                if (ScoreGateGeometryUtils.DoSegmentsIntersect(previousPosition, currentPosition, gapStart, gapEnd))
                {
                    ScorePass(player, scoreGate.Id);
                }
            }
        }

        private void ScorePass(PlayerStateS2C player, ushort scoreGateId)
        {
            var simulationState = _matchDataService.SimulationState;
            var gatePassConfig = _gamePlayConfigService.GamePlayConfig.GatePass;

            ref var scoreGate = ref simulationState.GetScoreGateById(scoreGateId);
            
            var isSameTeamStreak = scoreGate.LastScoredTeamId == player.TeamId;
            var multiplier = isSameTeamStreak ? scoreGate.ScoreMultiplier : (ushort)1;
            var score =(ushort)(gatePassConfig.ScorePerPass * multiplier);

            simulationState.AddStageScoreForTeam(player.TeamId, score);
            var teamBonusScoreTotal = simulationState.StageScorePerTeamId[player.TeamId];
            var byPlayerBonusScoreTotal = simulationState.AddStageScoreForPlayer(player.Id, score);

            // The stored multiplier is what the NEXT pass will award, so it ratchets up after each scored pass and drives
            // the client's x2/x3/x4 indicator.
            var nextMultiplier = (ushort) Math.Min(multiplier + 1, gatePassConfig.MaxGatePassMultiplier);
            scoreGate.LastScoredTeamId = player.TeamId;
            scoreGate.ScoreMultiplier = nextMultiplier;

            var cooldownTicks = (int)MathF.Ceiling(gatePassConfig.PassScoreCooldownSeconds * _networkConfig.TicksPerSeconds);
            _playersPassedScoreGateTrackerService.StartPlayerPassScoreCooldown(player.Id, scoreGateId, _processedTick + cooldownTicks);

            _netEventsDataService.AddPlayerPassedScoreGateNetEvent(_processedTick, scoreGateId, player.Id, (byte)score, nextMultiplier, (ushort)teamBonusScoreTotal, (ushort)byPlayerBonusScoreTotal);
        }
    }
}
