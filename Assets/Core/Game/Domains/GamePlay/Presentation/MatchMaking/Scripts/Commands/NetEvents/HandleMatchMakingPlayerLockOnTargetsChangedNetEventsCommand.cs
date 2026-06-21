using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.NetEvents
{
    public class HandleMatchMakingPlayerLockOnTargetsChangedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private static readonly Vector2 WALL_CENTER = Vector2.zero;

        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchMakingDataService _matchMakingDataService;
        private IMatchMakingPlayerControllers _playerControllers;
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        private ILockOnTargetShootEffectController _lockOnTargetShootEffectController;

        private readonly Dictionary<ushort, bool> _wasShootablePerPlayer = new Dictionary<ushort, bool>();

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
            _lockOnTargetShootEffectController = _diContainer.Resolve<ILockOnTargetShootEffectController>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.PlayerLockOnHeartTargetsChangedNetEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in _cachedPresentationEventsService.PlayerLockOnHeartTargetsChangedNetEvents)
            {
                var playerId = netEvent.PlayerId;
                var targets = netEvent.PlayerIdsLockedOnTarget;
                var isLockingOnWall = targets.Count > 0;
                var isWallShootable = isLockingOnWall && targets[0].IsLockOnTargetShootable;

                _playerControllers.SetIsLockOnHeartSightShownForPlayer(playerId, isLockingOnWall);
                _lockOnTargetEffectController.RefreshTargetEffectsOfCaster(playerId, targets);

                var wasShootable = _wasShootablePerPlayer.TryGetValue(playerId, out var previousShootable) && previousShootable;
                var didJustShoot = isLockingOnWall && wasShootable && !isWallShootable;
                if (didJustShoot)
                {
                    var headPosition = _matchMakingDataService.GetPlayer(playerId).Spaceship.Transform.GetHeadPosition().ToUnityVector2();
                    _lockOnTargetShootEffectController.Play(headPosition, WALL_CENTER);
                }

                _wasShootablePerPlayer[playerId] = isWallShootable;
                netEvent.PlayerIdsLockedOnTarget.Clear();
            }

            _cachedPresentationEventsService.PlayerLockOnHeartTargetsChangedNetEvents.Clear();
        }
    }
}
