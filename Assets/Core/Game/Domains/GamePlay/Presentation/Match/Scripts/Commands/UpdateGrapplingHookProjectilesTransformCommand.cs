using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands
{
    public class UpdateGrapplingHookProjectilesTransformCommand : BaseCommand, ICommandVoid
    {
        private IGrapplingHookProjectilesControllers _hookControllers;
        private IMatchPlayerControllers _playerControllers;
        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _hookControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            foreach (var hookModel in _matchDataService.GrapplingHookProjectiles)
            {
                var casterPosition = _playerControllers.GetPlayerPosition(hookModel.CasterPlayerId);
                var rotation = Quaternion.identity; // Can compute proper rotation to face player if necessary, or let view handle it

                // Rotate towards opposite direction of player
                Vector2 directionFromCaster = (hookModel.Position - casterPosition).normalized;
                if (directionFromCaster != Vector2.zero)
                {
                    float angle = Mathf.Atan2(directionFromCaster.y, directionFromCaster.x) * Mathf.Rad2Deg;
                    rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }

                _hookControllers.InterpolateGrapplingHookTransform(hookModel.Id, hookModel.Position, rotation, casterPosition);
            }
        }
    }
}
