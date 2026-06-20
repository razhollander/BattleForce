using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands
{
    public class UpdateMatchMakingLockOnWallEffectsCommand : BaseCommand, ICommandVoid
    {
        private IMatchMakingDataService _matchMakingDataService;
        private IMatchMakingPlayerControllers _playerControllers;
        private ILockOnTargetEffectController _lockOnTargetEffectController;
        private ILockOnTargetShootEffectController _lockOnTargetShootEffectController;
        private SharedGamePlayConfig _sharedGamePlayConfig;

        private ushort _wallTargetId;
        private FixedUnorderedList<PlayerOnTargetS2C> _cachedWallTargets;
        private readonly HashSet<ushort> _addedPlayers = new HashSet<ushort>();
        private readonly Dictionary<ushort, bool> _wasShootablePerPlayer = new Dictionary<ushort, bool>();

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
            _lockOnTargetShootEffectController = _diContainer.Resolve<ILockOnTargetShootEffectController>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _wallTargetId = _sharedGamePlayConfig.MinEntityId;
            _cachedWallTargets = new FixedUnorderedList<PlayerOnTargetS2C>(1);
        }

        public void Execute()
        {
            var wallCenter = Vector2.zero;

            foreach (var player in _matchMakingDataService.Players)
            {
                var playerId = player.PlayerId;
                EnsurePlayerAdded(playerId);

                var isLockingOnWall = player.Spaceship.IsLockingOnWall;
                var isWallShootable = player.Spaceship.IsLockingOnWallShootable;

                _cachedWallTargets.Clear();
                if (isLockingOnWall)
                {
                    ref var target = ref _cachedWallTargets.AddAndGet();
                    target.PlayerTargetId = _wallTargetId;
                    target.IsLockOnTargetShootable = isWallShootable;
                }

                _lockOnTargetEffectController.RefreshTargetEffectsOfCaster(playerId, _cachedWallTargets);

                if (isLockingOnWall)
                {
                    var headPosition = ToUnityVector2(player.Spaceship.Transform.GetHeadPosition());
                    _lockOnTargetEffectController.UpdateTargetsPositionOnPlayer(playerId, _wallTargetId, headPosition, wallCenter);

                    var wasShootable = _wasShootablePerPlayer.TryGetValue(playerId, out var previousShootable) && previousShootable;
                    var didJustShoot = wasShootable && !isWallShootable;
                    if (didJustShoot)
                    {
                        _lockOnTargetShootEffectController.Play(headPosition, wallCenter);
                    }
                }

                _wasShootablePerPlayer[playerId] = isWallShootable;
                _playerControllers.SetIsLockOnHeartSightShownForPlayer(playerId, isLockingOnWall);
            }
        }

        private void EnsurePlayerAdded(ushort playerId)
        {
            if (!_addedPlayers.Add(playerId))
            {
                return;
            }

            _cachedWallTargets.Clear();
            _lockOnTargetEffectController.AddPlayer(playerId, _cachedWallTargets);
        }

        private static Vector2 ToUnityVector2(System.Numerics.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
