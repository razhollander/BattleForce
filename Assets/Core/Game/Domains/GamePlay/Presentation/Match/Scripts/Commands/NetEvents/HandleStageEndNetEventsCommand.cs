using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Services.AudioService;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleStageEndNetEventsCommand : BaseCommand, ICommandVoid
    {
        private const float WINNER_ZOOM_MULTIPLIER = 0.2f;
        private const float WINNER_ZOOM_DURATION_SECONDS = 1.5f;

        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IStageEndedUiController _stageEndedUiController;
        private IWorldCameraController _worldCameraController;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _stageEndedUiController = _diContainer.Resolve<IStageEndedUiController>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            var stageEndEvents = _cachedPresentationEventsService.StageEndNetEvents;
            if (stageEndEvents.IsNullOrEmpty())
            {
                return;
            }

            _audioService.PlayAudio(AudioClipType.StageWinLaugh);
            
            foreach (var stageEndEvent in stageEndEvents)
            {
                var winningTeamId = stageEndEvent.WinningTeamId;
                var isThereOnlyOneTeam = winningTeamId==0;

                if(!isThereOnlyOneTeam)
                {
                    _stageEndedUiController.Show(winningTeamId, stageEndEvent.JemsWonPerTeam);
                    _worldCameraController.ShakeCamera(10,0.5f);
                    SetPlayersInTeamKinged();
                    ZoomCameraOnLastAlivePlayer();
                }
            }

            stageEndEvents.Clear();
        }

        private void SetPlayersInTeamKinged()
        {
            if (!_matchDataService.TryGetKingedPlayers(out var kingedPlayers))
            {
                return;
            }

            foreach (var playerModel in kingedPlayers)
            {
                _matchPlayerControllers.SetIsPlayerKinged(playerModel.PlayerId, true);
            }
        }

        // Zoom on the last surviving player (first kinged player). Reset on next stage start via SyncMatchSimulationStateCommand.
        private void ZoomCameraOnLastAlivePlayer()
        {
            _worldCameraController.ClearTargets();

            if (!_matchDataService.TryGetKingedPlayers(out var kingedPlayers) || kingedPlayers.Count == 0)
            {
                return;
            }

            _worldCameraController.AddFollowTarget(_matchPlayerControllers.GetPlayerTransform(kingedPlayers[0].PlayerId));
            _worldCameraController.LerpOrthographicSizeMultiplier(WINNER_ZOOM_MULTIPLIER, WINNER_ZOOM_DURATION_SECONDS);
        }
    }
}
