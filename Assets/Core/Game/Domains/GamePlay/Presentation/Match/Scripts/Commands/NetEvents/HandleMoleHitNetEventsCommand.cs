using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Extensions;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleMoleHitNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IMoleControllers _moleControllers;
        private ITeamsBoardUIController _teamsBoardUIController;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
            _teamsBoardUIController = _diContainer.Resolve<ITeamsBoardUIController>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var moleHitNetEvents = _cachedPresentationEventsService.MoleHitNetEvents;

            if (moleHitNetEvents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var moleHitNetEvent in moleHitNetEvents)
            {
                _moleControllers.SetMoleHit(moleHitNetEvent.MoleId);
                _teamsBoardUIController.UpdateTeamMolesHit(moleHitNetEvent.ByTeamId, moleHitNetEvent.TeamMolesHitTotal);
            }

            _audioService.PlayAudio(AudioClipType.MoleHit);
            moleHitNetEvents.Clear();
        }
    }
}
