using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.GrapplingHook.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleCreateGrapplingHookProjecitleNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IGrapplingHookProjectilesControllers _hookProjectilesControllers;
        private IMatchPlayerControllers _playerControllers;
        private IAudioService _audioService;

        private IMatchDataService _matchDataService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _hookProjectilesControllers = _diContainer.Resolve<IGrapplingHookProjectilesControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.CreateGrapplingHookProjectileNetEvents;
            if (netEvents.Count == 0)
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                var hookModel = netEvent.GrapplingHookProjectile;
                var casterPlayerId = hookModel.PlayerCasterId;
                var casterPosition = _playerControllers.GetPlayerPosition(casterPlayerId);
                var rotation = hookModel.Position - casterPosition.ToNumericsVector2();

                _hookProjectilesControllers.CreateGrapplingHookProjectile(hookModel.Id, casterPlayerId, hookModel.Position.ToUnityVector2(), rotation.ToUnityVector2(),
                    casterPosition, hookModel.IsHookAttached);
                _audioService.PlayAudio(AudioClipType.TalentCast);
            }

            _cachedPresentationEventsService.CreateGrapplingHookProjectileNetEvents.Clear();
        }
    }
}
