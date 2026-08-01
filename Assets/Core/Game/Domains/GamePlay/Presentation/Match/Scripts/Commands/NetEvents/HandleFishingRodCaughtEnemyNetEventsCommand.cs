using Core.Game.Domains.GamePlay.Presentation.Match.Features.FishingRod.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.SecondCastAimArrowEffect.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleFishingRodCaughtEnemyNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchDataService _matchDataService;
        private IFishingRodTipControllers _fishingRodTipControllers;
        private ISecondCastAimArrowControllers _secondCastAimArrowControllers;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _fishingRodTipControllers = _diContainer.Resolve<IFishingRodTipControllers>();
            _secondCastAimArrowControllers = _diContainer.Resolve<ISecondCastAimArrowControllers>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.FishingRodCaughtEnemyNetEvents;
            if (events.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var caughtEvent in events)
            {
                _fishingRodTipControllers.StopFishingRodTipReelLoopAudio(caughtEvent.ProjectileId);
                var tipModel = _matchDataService.GetFishingRodTip(caughtEvent.ProjectileId);
                _secondCastAimArrowControllers.AddArrow(tipModel.Id, tipModel.Position, tipModel.EnemyCaughtArrowDirection);
            }

            _audioService.PlayAudio(AudioClipType.FishingRodCatch);
            _cachedPresentationEventsService.FishingRodCaughtEnemyNetEvents.Clear();
        }
    }
}
