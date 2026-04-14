using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
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
                var directionFromCaster = hookModel.Position - casterPosition;
                var rotation = directionFromCaster.ToQuaternion();
                _hookControllers.InterpolateGrapplingHookTransform(hookModel.Id, hookModel.Position, rotation, casterPosition);
            }
        }
    }
}
