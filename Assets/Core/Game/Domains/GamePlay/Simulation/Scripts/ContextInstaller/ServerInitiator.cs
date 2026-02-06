using System.Threading;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Scripts.Services.ApplicationSubscriptionService;
using Core.Scripts.Utils;
using CoreDomain.Scripts.CoreInitiator.Base;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.InitiatorInvokerService;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.ContextInstaller
{
    public class ServerInitiator : ISceneInitiator, IServerInitiator, IApplicationObserver
    {
        private readonly ICommandFactory _commandFactory;
        private readonly ISceneInitiatorsService _sceneInitiatorsService;
        private readonly IApplicationSubscriptionService _applicationSubscriptionService;
        private readonly IPlaybackRecorderService _playbackRecorderService;
        private readonly ServerEntryPointCommand _serverEntryPointCommand;
        private readonly ServerExitPointCommand _serverExitPointCommand;

        public SceneType SceneType => SceneType.ServerScene;

        public ServerInitiator(ICommandFactory commandFactory, ISceneInitiatorsService sceneInitiatorsService, IApplicationSubscriptionService applicationSubscriptionService, IPlaybackRecorderService playbackRecorderService)
        {
            _commandFactory = commandFactory;
            _sceneInitiatorsService = sceneInitiatorsService;
            _applicationSubscriptionService = applicationSubscriptionService;
            _playbackRecorderService = playbackRecorderService;
            _applicationSubscriptionService.RegisterObserver(this);
            _sceneInitiatorsService.RegisterInitiator(this);
            _serverEntryPointCommand = _commandFactory.CreateCommandVoid<ServerEntryPointCommand>();
            _serverExitPointCommand = _commandFactory.CreateCommandVoid<ServerExitPointCommand>();
        }

        public Awaitable LoadEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            return AwaitableUtils.CompletedTask;
        }

        public Awaitable StartEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            var serverInitiatorEnterData = (ServerInitiatorEnterData) enterDataObject;
            if (serverInitiatorEnterData != null)
            {
                _playbackRecorderService.SetPlaybackInfo(serverInitiatorEnterData.IsPlaybackEnabled, serverInitiatorEnterData.PlaybackFileName);
            }
            _serverEntryPointCommand.Execute();
            return AwaitableUtils.CompletedTask;
        }

        public Awaitable InitExitPoint(CancellationTokenSource cancellationTokenSource)
        {
            InitExitPoint();
            return AwaitableUtils.CompletedTask;
        }

        private void InitExitPoint()
        {
            _sceneInitiatorsService.UnregisterInitiator(this);
            _applicationSubscriptionService.UnregisterObserver(this);
            _serverExitPointCommand.Execute();
        }

        public void OnApplicationQuit()
        {
            InitExitPoint();
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            
        }
    }
}