using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.StageCancellationToken;
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Mvc.WorldCamera;
using Core.Scripts.Services.AudioService;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Mvc.WorldCamera;
using CoreDomain.Scripts.Services.CommandFactory;
using Sirenix.Utilities;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleStageEndNetEventsCommand : BaseCommand, ICommandVoid
    {
        private const float WINNER_ZOOM_MULTIPLIER = 0.3f;
        private const float WINNER_ZOOM_DURATION_SECONDS = 1f;

        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IStageEndedUiController _stageEndedUiController;
        private IWorldCameraController _worldCameraController;
        private IMatchPlayerControllers _matchPlayerControllers;
        private IMatchDataService _matchDataService;
        private IAudioService _audioService;
        private IStageCancellationTokenProvider _stageCancellationTokenProvider;
        private IMoleControllers _moleControllers;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _stageEndedUiController = _diContainer.Resolve<IStageEndedUiController>();
            _worldCameraController = _diContainer.Resolve<IWorldCameraController>();
            _matchPlayerControllers = _diContainer.Resolve<IMatchPlayerControllers>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _audioService = _diContainer.Resolve<IAudioService>();
            _stageCancellationTokenProvider = _diContainer.Resolve<IStageCancellationTokenProvider>();
            _moleControllers = _diContainer.Resolve<IMoleControllers>();
        }

        public void Execute()
        {
            var stageEndEvents = _cachedPresentationEventsService.StageEndNetEvents;
            if (stageEndEvents.IsNullOrEmpty())
            {
                return;
            }

            _audioService.PlayAudio(AudioClipType.StageWinLaugh);

            // The server hides all moles at the timer end without per-mole net events, so the client mirrors that here.
            if (_matchDataService.StageType == StageType.WhacAMole)
            {
                _moleControllers.SetAllMolesInHole();
            }

            foreach (var stageEndEvent in stageEndEvents)
            {
                var winningTeamId = stageEndEvent.WinningTeamId;
                var isThereOnlyOneTeam = winningTeamId==0;

                if (isThereOnlyOneTeam)
                {
                    continue;
                }

                _stageEndedUiController.Show(winningTeamId, stageEndEvent.JemsWonPerTeam);
                _worldCameraController.ShakeCamera(10f,0.5f);
            }
            
            if (_matchDataService.TryGetKingedPlayers(out var kingedPlayers) && !kingedPlayers.IsNullOrEmpty())
            {
                SetPlayersKinged(kingedPlayers);
            }

            // The camera locks onto the player the server picked - the one doing the winning blow, or in bonus stages
            // the winning-team player who contributed the most score.
            foreach (var stageEndEvent in stageEndEvents)
            {
                var isThereOnlyOneTeam = stageEndEvent.WinningTeamId == 0;

                if (isThereOnlyOneTeam)
                {
                    continue;
                }

                ZoomCameraOnPlayer(stageEndEvent.PlayerIdToFocusOn);
            }

            stageEndEvents.Clear();
        }

        private void SetPlayersKinged(List<MatchPlayerModel> kingedPlayers)
        {
            foreach (var playerModel in kingedPlayers)
            {
                _matchPlayerControllers.SetIsPlayerKinged(playerModel.PlayerId, true);
            }
        }

        private void ZoomCameraOnPlayer(ushort playerId)
        {
            _worldCameraController.ClearTargets();
            _worldCameraController.AddFollowTarget(_matchPlayerControllers.GetPlayerTransform(playerId));
            _worldCameraController.LerpOrthographicSizeMultiplier(WINNER_ZOOM_MULTIPLIER, WINNER_ZOOM_DURATION_SECONDS, _stageCancellationTokenProvider.CancellationTokenSource.Token).Forget();
            _worldCameraController.SetisDampingEnabled(false);
        }
    }
}
