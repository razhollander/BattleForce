using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.AudioService;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleLayChickenEggNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchChickenEggsControllers _chickenEggsControllers;
        private IMatchDataService _matchDataService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _chickenEggsControllers = _diContainer.Resolve<IMatchChickenEggsControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var netEvents = _cachedPresentationEventsService.LayChickenEggNetEvents;
            if (netEvents.IsNullOrEmpty()) return;

            foreach (var netEvent in netEvents)
            {
                var casterPlayerId = netEvent.CasterPlayerId;
                var playerCasterTeamId = _matchDataService.GetPlayerTeamId(casterPlayerId);
                _matchPlayerControllers.PlayLayEggAnimation(casterPlayerId);
                _chickenEggsControllers.CreateEgg(netEvent.EggId, netEvent.Position, playerCasterTeamId);
                _audioService.PlayAudio(AudioClipType.ChickenEggLay, AudioChannelType.Fx);
            }

            netEvents.Clear();
        }
    }
}
