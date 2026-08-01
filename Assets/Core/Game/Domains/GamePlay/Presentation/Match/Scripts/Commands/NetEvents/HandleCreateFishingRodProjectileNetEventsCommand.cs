using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleCreateFishingRodProjectileNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IFishingRodTipControllers _fishingRodTipControllers;
        private IMatchPlayerControllers _playerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.CreateFishingRodProjectileNetEvents;
            if (netEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in netEvents)
            {
                var tipModel = netEvent.FishingRodProjectile;
                var casterPlayerId = tipModel.PlayerCasterId;
                
                _playerControllers.SetPlayerFishingRodStickState(casterPlayerId, true);
                var casterPosition = _playerControllers.GetPlayerPosition(casterPlayerId);
                var lineStartPosition = _playerControllers.GetPlayerFishingRodTipPivotPosition(casterPlayerId);
                var rotation = tipModel.Position - casterPosition.ToNumericsVector2();
                _fishingRodTipControllers.CreateFishingRodTip(tipModel.Id, tipModel.Position.ToUnityVector2(), rotation.ToUnityVector2(), lineStartPosition, tipModel.Phase);
            }

            _audioService.PlayAudio(AudioClipType.FishingRodActivate);

            _cachedPresentationEventsService.CreateFishingRodProjectileNetEvents.Clear();
        }
    }
}
