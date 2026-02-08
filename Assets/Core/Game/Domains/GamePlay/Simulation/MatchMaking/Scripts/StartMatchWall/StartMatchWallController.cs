using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchMakingModel.MatchMakingModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.States;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall
{
    public class StartMatchWallController : IStartMatchWallController
    {
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly ISimulationStateMachine _simulationStateMachine;
        private readonly IMatchMakingDataService _matchMakingDataService;

        private bool _isCountingDown;
        private bool _isEligible = true;
        private float _countdownTimer;
        private int _lastTickGotHitByBullet = -1;
        private const ushort START_MATCH_WALL_ID = 1;
        public bool DidFinishCountingDown => _isCountingDown && _countdownTimer <= 0;
        public StartMatchWallController(IPhysicsSimulator physicsSimulator, INetEventsDataService netEventsDataService, SimulationGamePlayConfig gamePlayConfig, ISimulationStateMachine simulationStateMachine, IMatchMakingDataService matchMakingDataService)
        {
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfig = gamePlayConfig;
            _simulationStateMachine = simulationStateMachine;
            _matchMakingDataService = matchMakingDataService;
        }

        public void Initialize(float radius)
        {
            _physicsSimulator.AddStartMatchWall(START_MATCH_WALL_ID, Vector2.Zero, radius);
        }

        public void TryToggleState(int tick)
        {
            var wasAlreadyHitByBulletThisTick = _lastTickGotHitByBullet == tick;
            if (wasAlreadyHitByBulletThisTick)
            {
                return;
            }
            
            _lastTickGotHitByBullet = tick;

            if (_isCountingDown)
            {
                StopCountdown(tick);
            }
            else
            {
                if (_isEligible)
                {
                    StartCountdown(tick);
                }
            }
        }

        private void StartCountdown(int tick)
        {
            _isCountingDown = true;
            _countdownTimer = _gamePlayConfig.StartMatchCountdownDuration;
            _netEventsDataService.AddStartMatchCountdownNetEvent(tick, _gamePlayConfig.StartMatchCountdownDuration);
            LogService.LogTopic($"Start Match Countdown started: {_countdownTimer}s", LogTopicType.ServerNetwork);
        }

        public void TryStopCountdown(int tick)
        {
            var wasAlreadyHitByBulletThisTick = _lastTickGotHitByBullet == tick;
            if (!_isCountingDown || wasAlreadyHitByBulletThisTick)
            {
                return;
            }
            
            StopCountdown(tick);
        }
        
        private void StopCountdown(int tick)
        {
            _isCountingDown = false;
            _countdownTimer = 0;
            _netEventsDataService.AddStopMatchCountdownNetEvent(tick);
             LogService.LogTopic("Start Match Countdown stopped", LogTopicType.ServerNetwork);
        }

        public void StepTimer(float deltaTime)
        {
            if (!_isCountingDown)
            {
                return;
            }

            _countdownTimer -= deltaTime;
        }

        public void OnPlayerTeamChanged(int tick)
        {
            var isEligible = CheckEligibility();
            if (isEligible != _isEligible)
            {
                _isEligible = isEligible;
                _netEventsDataService.AddStartMatchEligibleChangedNetEvent(tick, _isEligible);
            }

            if (!_isEligible)
            {
                TryStopCountdown(tick);
            }
            else
            {
                // Previously logic was to stop countdown on any team change.
                // Assuming we want to maintain that behavior:
                TryStopCountdown(tick);
            }
        }

        private bool CheckEligibility()
        {
            var players = _matchMakingDataService.SimulationState.Players;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].TeamId == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
