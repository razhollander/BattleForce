using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleDeactivateFishingRodTalentNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IFishingRodTipControllers _fishingRodTipControllers;
        private ISecondCastEffectController _secondCastEffectController;
        private IMatchDataService _matchDataService;
        private IMatchPlayerControllers _playerControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _secondCastEffectController = _diContainer.Resolve<ISecondCastEffectController>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.DeactivateFishingRodTalentNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }

            foreach (var netEvent in events)
            {
                // The projectile is gone, so remove its throw-aim arrow (if any) along with the tip.
                _secondCastEffectController.RemoveArrow(netEvent.ProjectileId);
                _fishingRodTipControllers.DestroyFishingRodTip(netEvent.ProjectileId);
                _matchDataService.RemoveFishingRodTip(netEvent.ProjectileId);
                _playerControllers.SetPlayerFishingRodStickState(netEvent.CasterPlayerId, false);
            }

            _audioService.StopLoopAudio(AudioClipType.FishingRodReel);

            _cachedPresentationEventsService.DeactivateFishingRodTalentNetEvents.Clear();
        }
    }
}
