using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleTalentSwitchNetEventsCommand : BaseCommand, ICommandVoid
    {
        private IMatchPlayerControllers _playerControllers;
        private IMatchPlayerUIControllers _playerUIControllers;
        private ICachedPresentationEventsService _cachedPresentationEventsService;

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _playerUIControllers = _diContainer.Resolve<IMatchPlayerUIControllers>();
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
        }

        public void Execute()
        {
            var talentSwitchEvents = _cachedPresentationEventsService.TalentSwitchNetEvents;
            if (talentSwitchEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var talentSwitchEvent in talentSwitchEvents)
            {
                var playerId = talentSwitchEvent.PlayerId;
                var newTalentIndex = talentSwitchEvent.NewTalentIndex;
                _playerControllers.SetPlayerTalentSelected(playerId, newTalentIndex);
                _playerUIControllers.SetPlayerSelectedTalent(playerId, newTalentIndex);
            }

            talentSwitchEvents.Clear();
        }
    }
}