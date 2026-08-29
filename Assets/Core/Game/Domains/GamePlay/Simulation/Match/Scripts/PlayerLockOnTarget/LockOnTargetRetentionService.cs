using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget
{
    public class LockOnTargetRetentionService : ILockOnTargetRetentionService
    {
        private readonly IMatchDataService _matchDataService;
        private readonly ILockOnTargetTimerService _lockOnTargetTimerService;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly NetworkConfig _networkConfig;

        public LockOnTargetRetentionService(IMatchDataService matchDataService, ILockOnTargetTimerService lockOnTargetTimerService,
            SharedGamePlayConfig sharedGamePlayConfig, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _lockOnTargetTimerService = lockOnTargetTimerService;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _networkConfig = networkConfig;
        }

        public void AddRetainedTargets(PlayerStateS2C casterPlayerState, FixedUnorderedList<ObjectLockedOnTargetS2C> targetsInConeSight, int processedTick)
        {
            var previouslyTargetedObjects = casterPlayerState.Spaceship.LockOnTargetObjects;

            for (int i = 0; i < previouslyTargetedObjects.Count; i++)
            {
                var previouslyTargetedObject = previouslyTargetedObjects[i];
                if (!TryGetRetentionEndTick(casterPlayerState.Id, previouslyTargetedObject, targetsInConeSight, processedTick, out var retentionEndTick))
                {
                    continue;
                }

                ref var retainedTarget = ref targetsInConeSight.AddAndGet();
                retainedTarget.TargetId = previouslyTargetedObject.TargetId;
                retainedTarget.TargetType = previouslyTargetedObject.TargetType;
                retainedTarget.IsLockOnTargetShootable = true;
                retainedTarget.RetentionEndTick = retentionEndTick;
            }
        }

        private bool TryGetRetentionEndTick(ushort casterId, ObjectLockedOnTargetS2C previouslyTargetedObject,
            FixedUnorderedList<ObjectLockedOnTargetS2C> targetsInConeSight, int processedTick, out int retentionEndTick)
        {
            retentionEndTick = ObjectLockedOnTargetS2C.NO_RETENTION_END_TICK;
            var targetKey = previouslyTargetedObject.GetKey();
            
            var isTargetStillInConeSight = targetsInConeSight.ContainsTarget(targetKey);
            if (isTargetStillInConeSight)
            {
                return false;
            }

            var wasTargetShootable = _lockOnTargetTimerService.IsTargetShootable(casterId, targetKey.TargetId, targetKey.TargetType);
            if (!wasTargetShootable || !DoesTargetStillExist(previouslyTargetedObject))
            {
                return false;
            }

            retentionEndTick = previouslyTargetedObject.IsLockOnTargetRetained
                ? previouslyTargetedObject.RetentionEndTick
                : TickUtils.GetTickPassedAfterDuration(processedTick, _sharedGamePlayConfig.LockOnTargetRetentionDurationInSeconds, _networkConfig.DeltaTime);

            var hasRetentionEnded = processedTick >= retentionEndTick;
            return !hasRetentionEnded;
        }

        private bool DoesTargetStillExist(ObjectLockedOnTargetS2C targetedObject)
        {
            var simulationState = _matchDataService.SimulationState;

            switch (targetedObject.TargetType)
            {
                case LockOnTargetType.Heart:
                    return simulationState.TryGetPlayerById(targetedObject.TargetId, out var targetPlayerState) && targetPlayerState.Spaceship.IsAlive;
                case LockOnTargetType.PowerUpBall:
                    return simulationState.TryGetPowerUpBallIndexById(targetedObject.TargetId, out _);
                case LockOnTargetType.Mole:
                    return simulationState.TryGetMoleByHoleId(targetedObject.TargetId, out _);
                default:
                    return false;
            }
        }
    }
}
