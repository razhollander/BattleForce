using System.Threading;
using Core.Scripts.Services.ApplicationSubscriptionService;
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

        public SceneType SceneType => SceneType.ServerScene;

        public ServerInitiator(ICommandFactory commandFactory, ISceneInitiatorsService sceneInitiatorsService, IApplicationSubscriptionService applicationSubscriptionService)
        {
            _commandFactory = commandFactory;
            _sceneInitiatorsService = sceneInitiatorsService;
            _applicationSubscriptionService = applicationSubscriptionService;
            _applicationSubscriptionService.RegisterObserver(this);
            _sceneInitiatorsService.RegisterInitiator(this);
        }

        public Awaitable LoadEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            return AwaitableUtils.CompletedTask;
        }

        public Awaitable StartEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            _commandFactory.CreateCommandVoid<ServerEntryPointCommand>().Execute();
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
            _commandFactory.CreateCommandVoid<ServerExitPointCommand>().Execute();
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