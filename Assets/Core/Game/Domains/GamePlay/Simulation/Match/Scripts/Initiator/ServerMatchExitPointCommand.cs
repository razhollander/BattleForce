using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator
{
    public class ServerMatchExitPointCommand: BaseCommand, ICommandVoid
    {
        private IMatchPlayerJoinPacketsHandler _matchPlayerJoinPacketsHandler;
        private ITickProcessor _tickProcessor;
        private IMatchPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private IPlaybackRecorderService _playbackRecorderService;

        public override void ResolveDependencies()
        {
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _matchPlayerJoinPacketsHandler = _diContainer.Resolve<IMatchPlayerJoinPacketsHandler>();
            _playerInputsPacketsHandler = _diContainer.Resolve<IMatchPlayerInputsPacketsHandler>();
            _playbackRecorderService = _diContainer.Resolve<IPlaybackRecorderService>();
        }

        public void Execute()
        {
            if (!_playbackRecorderService.IsPlaybackEnabled)
            {
                _playbackRecorderService.StopRecording();
            }
            _matchPlayerJoinPacketsHandler.InitExitPoint();
            _tickProcessor.InitExitPoint();
            _playerInputsPacketsHandler.InitExitPoint();
        }
    }
}