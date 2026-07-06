using Core.Game.Domains.GamePlay.Presentation.Features.LockOnTarget;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands
{
    public class UpdateMatchMakingLockOnWallEffectsCommand : BaseCommand, ICommandVoid
    {
        private static readonly Vector2 WALL_CENTER = Vector2.zero;

        private IMatchMakingDataService _matchMakingDataService;
        private ILockOnTargetEffectController _lockOnTargetEffectController;

        public override void ResolveDependencies()
        {
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _lockOnTargetEffectController = _diContainer.Resolve<ILockOnTargetEffectController>();
        }

        public void Execute()
        {
            foreach (var player in _matchMakingDataService.Players)
            {
                var wallTargets = player.Spaceship.ObjectsLockedOnTarget;
                if (wallTargets.Count == 0)
                {
                    continue;
                }

                var headPosition = player.Spaceship.Transform.GetHeadPosition().ToUnityVector2();
                _lockOnTargetEffectController.UpdateTargetsPositionOnPlayer(player.PlayerId, wallTargets[0].GetKey(), headPosition, WALL_CENTER);
            }
        }
    }
}
