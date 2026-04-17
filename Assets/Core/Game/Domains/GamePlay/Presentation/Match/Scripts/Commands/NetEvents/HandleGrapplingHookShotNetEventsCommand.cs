using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleGrapplingHookShotNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IGrapplingHookProjectilesControllers _hookProjectilesControllers;
        private IMatchPlayerControllers _playerControllers;

        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _hookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.PlayerGrapplingHookShotNetEvents;
            if (netEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                var hookModel = netEvent.GrapplingHookProjectile;
                var casterPosition = _playerControllers.GetPlayerPosition(hookModel.PlayerCasterId);
                var rotation = hookModel.Position - casterPosition.ToNumericsVector2();

                _hookProjectilesControllers.CreateGrapplingHookProjectile(hookModel.Id, hookModel.PlayerCasterId, hookModel.Position.ToUnityVector2(), rotation.ToUnityVector2(), casterPosition, hookModel.IsHookAttached);
                _matchDataService.AddGrapplingHookProjectile(hookModel.Id, hookModel.PlayerCasterId, hookModel.Position);

                // // Hide the aim arrow
                // var casterPlayerController = _playerControllers.GetPlayerController(hookModel.PlayerCasterId);
                // if (casterPlayerController != null)
                // {
                //     casterPlayerController.SetAimViewActive(false);
                // }
            }

            _cachedPresentationEventsService.PlayerGrapplingHookShotNetEvents.Clear();
        }
    }
}
