using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.StartMatchWall
{
    public class StartMatchWallController : IStartMatchWallController
    {
        private readonly IPhysicsSimulator _physicsSimulator;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;

        private bool _isCountingDown;
        private float _countdownTimer;
        private int _lastTickGotHitByBullet = -1;
        
        public bool DidFinishCountingDown => _isCountingDown && _countdownTimer <= 0;
        public StartMatchWallController(IPhysicsSimulator physicsSimulator, INetEventsDataService netEventsDataService, ISimulationGamePlayConfigService gamePlayConfigService, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _physicsSimulator = physicsSimulator;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void Initialize(float radius)
        {
            _physicsSimulator.AddStartMatchWall(_sharedGamePlayConfig.MinEntityId, Vector2.Zero, radius);
        }

        public void TryToggleCountdownState(int tick)
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
                StartCountdown(tick);
            }
        }

        private void StartCountdown(int tick)
        {
            _isCountingDown = true;
            _countdownTimer = _gamePlayConfigService.GamePlayConfig.StartMatchCountdownDuration;
            _netEventsDataService.AddStartMatchCountdownNetEvent(tick, _gamePlayConfigService.GamePlayConfig.StartMatchCountdownDuration);
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
    }
}
