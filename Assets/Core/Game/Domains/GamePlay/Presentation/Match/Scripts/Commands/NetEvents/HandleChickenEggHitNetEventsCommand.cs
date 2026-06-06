using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.AudioService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleChickenEggHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchChickenEggsControllers _chickenEggsControllers;
        private IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _stageCancellationTokenProvider = _diContainer.Resolve<IStageCancellationTokenProvider>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var events = _cachedPresentationEventsService.ChickenEggHitNetEvents;
            if (events.IsNullOrEmpty()) return;

            foreach (var evt in events)
            {
                _chickenEggsControllers.BreakAndDestroyEgg(evt.EggId, _stageCancellationTokenProvider.CancellationTokenSource).Forget();
                _audioService.PlayAudio(AudioClipType.ChickenEggHit, AudioChannelType.Fx);
            }

            events.Clear();
        }
    }
}
