using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall
{
    public class StartMatchWallController
    {
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;

        private bool _isCountingDown;
        private float _countdownTimer;
        private int _lastProcessedTick = -1;
        private const ushort START_MATCH_WALL_ID = 9999;

        public StartMatchWallController(IPhysicsSimulator physicsSimulator, INetEventsDataService netEventsDataService, SimulationGamePlayConfig gamePlayConfig)
        {
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfig = gamePlayConfig;
        }

        public void Initialize(float radius)
        {
            _physicsSimulator.AddStartMatchWall(START_MATCH_WALL_ID, Vector2.Zero, radius);
        }

        public void OnHitByBullet(int tick)
        {
            if (_lastProcessedTick == tick)
            {
                return;
            }
            _lastProcessedTick = tick;

            if (_isCountingDown)
            {
                StopCountdown(tick);
            }
            else
            {
                StartCountdown(tick);
            }
        }

        private void StartCountdown(int tick)
        {
            _isCountingDown = true;
            _countdownTimer = _gamePlayConfig.StartMatchCountdownDuration;
            _netEventsDataService.AddStartMatchCountdownNetEvent(tick, _countdownTimer);
            LogService.LogTopic($"Start Match Countdown started: {_countdownTimer}s", LogTopicType.ServerNetwork);
        }

        private void StopCountdown(int tick)
        {
            _isCountingDown = false;
            _countdownTimer = 0;
            _netEventsDataService.AddStopMatchCountdownNetEvent(tick);
             LogService.LogTopic("Start Match Countdown stopped", LogTopicType.ServerNetwork);
        }

        public void Tick(float deltaTime)
        {
            if (_isCountingDown)
            {
                _countdownTimer -= deltaTime;
                if (_countdownTimer <= 0)
                {
                    _isCountingDown = false;
                    _countdownTimer = 0;
                    // Do nothing when countdown finishes as per instructions.
                }
            }
        }
    }
}
