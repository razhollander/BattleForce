using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Commands.EntryPoint;
using Core.Scripts.Services.ApplicationSubscriptionService;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.CoreInitiator.Base;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.InitiatorInvokerService;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Initiator
{
    public class GamePlayInitiator : ISceneInitiator, IGamePlayInitiator, IApplicationObserver
    {
        private readonly ICommandFactory _commandFactory;
        private readonly ISceneInitiatorsService _sceneInitiatorsService;
        private readonly IApplicationSubscriptionService _applicationSubscriptionService;

        public SceneType SceneType => SceneType.GamePlayScene;

        public GamePlayInitiator(ICommandFactory commandFactory, ISceneInitiatorsService sceneInitiatorsService, IApplicationSubscriptionService applicationSubscriptionService)
        {
            _commandFactory = commandFactory;
            _sceneInitiatorsService = sceneInitiatorsService;
            _sceneInitiatorsService.RegisterInitiator(this);
            _applicationSubscriptionService = applicationSubscriptionService;
            _applicationSubscriptionService.RegisterObserver(this);
        }

        public async Awaitable LoadEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            var enterData = (GamePlayInitiatorEnterData)enterDataObject;
            await _commandFactory.CreateCommandAsync<LoadGamePlayStateCommand>().SetEnterData(enterData).Execute(cancellationTokenSource);
        }

        public async Awaitable StartEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            var enterData = (GamePlayInitiatorEnterData)enterDataObject;
            await _commandFactory.CreateCommandAsync<StartGamePlayStateCommand>().SetEnterData(enterData).Execute(cancellationTokenSource);
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
            _commandFactory.CreateCommandVoid<ExitGamePlayStateCommand>().Execute();
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