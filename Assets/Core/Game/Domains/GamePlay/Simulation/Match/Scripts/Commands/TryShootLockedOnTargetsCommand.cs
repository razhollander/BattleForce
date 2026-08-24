using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryShootLockedOnTargetsCommand : BaseCommand, ICommandVoid
    {
        private ICommandFactory _commandFactory;
        private IMatchDataService _matchDataService;
        private ILockOnTargetTimerService _lockOnTargetTimerService;
        private INetEventsDataService _netEventsDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private TryHitPlayerCommand _tryHitPlayerCommand;
        private ObtainPowerUpBallCommand _obtainPowerUpBallCommand;
        private TryHitMoleCommand _tryHitMoleCommand;

        private int _processedTick;
        private ushort _casterPlayerId;

        public TryShootLockedOnTargetsCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public TryShootLockedOnTargetsCommand SetCasterPlayerId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
            return this;
        }

        public override void ResolveDependencies()
        {
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _lockOnTargetTimerService = _diContainer.Resolve<ILockOnTargetTimerService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _tryHitPlayerCommand = _commandFactory.CreateCommandVoid<TryHitPlayerCommand>();
            _obtainPowerUpBallCommand = _commandFactory.CreateCommandVoid<ObtainPowerUpBallCommand>();
            _tryHitMoleCommand = _commandFactory.CreateCommandVoid<TryHitMoleCommand>();
        }

        public void Execute()
        {
            var casterState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var targetedEnemyIds = casterState.Spaceship.LockOnTargetObjects;

            for (int i = 0; i < targetedEnemyIds.Count; i++)
            {
                var target = targetedEnemyIds[i];
                var targetId = target.TargetId;
                if (!targetedEnemyIds[i].IsLockOnTargetShootable)
                {
                    continue;
                }

                _lockOnTargetTimerService.ResetTimer(_casterPlayerId, targetId, target.TargetType);

                switch (target.TargetType)
                {
                    case LockOnTargetType.Heart:
                        ShootHeartTarget(targetId);
                        break;
                    case LockOnTargetType.PowerUpBall:
                        ShootPowerUpBallTarget(targetId);
                        break;
                    case LockOnTargetType.Mole:
                        ShootMoleTarget(targetId);
                        break;
                }
            }
        }

        private void ShootHeartTarget(ushort targetId)
        {
            _tryHitPlayerCommand
                .SetPlayerIdGotHit(targetId)
                .SetWasHitByAnotherPlayer(true, _casterPlayerId)
                .SetProcessedTick(_processedTick)
                .SetHitDamage(_gamePlayConfigService.GamePlayConfig.PlayerSpaceship.LockOnTargetHitDamage)
                .Execute();

            _netEventsDataService.AddPlayerLockedOnTargetHitNetEvent(_processedTick, _casterPlayerId, targetId);
        }

        private void ShootMoleTarget(ushort moleHoleId)
        {
            if (!_matchDataService.SimulationState.TryGetMoleByHoleId(moleHoleId, out var mole))
            {
                return;
            }

            var casterTeamId = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId).TeamId;
            _tryHitMoleCommand
                .SetMoleId(mole.Id)
                .SetByPlayerId(_casterPlayerId)
                .SetByTeamId(casterTeamId)
                .SetProcessedTick(_processedTick)
                .Execute();
        }

        private void ShootPowerUpBallTarget(ushort powerUpBallId)
        {
            _obtainPowerUpBallCommand
                .SetProcessedTick(_processedTick)
                .SetPowerUpBallId(powerUpBallId)
                .SetObtainedByPlayerId(_casterPlayerId)
                .Execute();
        }
    }
}
