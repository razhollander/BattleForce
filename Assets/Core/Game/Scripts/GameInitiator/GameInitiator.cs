using System.Threading;
using CoreDomain.GameDomain.Scripts.States.GamePlayState;
using CoreDomain.Scripts.CoreInitiator.Base;
using CoreDomain.Scripts.Services.InitiatorInvokerService;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace CoreDomain.GameDomain.Scripts.GameInitiator
{
    public class GameInitiator : ISceneInitiator, IGameInitiator
    {
        private readonly IStateMachineService _stateMachine;
        private readonly ISceneInitiatorsService _sceneInitiatorsService;
        private readonly GamePlayState.Factory _gamePlayStateFactory;

        public SceneType SceneType => SceneType.GameScene;

        public GameInitiator(IStateMachineService stateMachine, ISceneInitiatorsService sceneInitiatorsService, GamePlayState.Factory gamePlayStateFactory)
        {
            _stateMachine = stateMachine;
            _sceneInitiatorsService = sceneInitiatorsService;
            _gamePlayStateFactory = gamePlayStateFactory;
            _sceneInitiatorsService.RegisterInitiator(this);
        }

        public async Awaitable LoadEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            await _stateMachine.EnterInitialGameState(_gamePlayStateFactory.Create(new GamePlayInitiatorEnterData()), cancellationTokenSource);
        }

        public Awaitable StartEntryPoint(IInitiatorEnterData enterDataObject, CancellationTokenSource cancellationTokenSource)
        {
            return AwaitableUtils.CompletedTask;
        }

        public Awaitable InitExitPoint(CancellationTokenSource cancellationTokenSource)
        {
            _sceneInitiatorsService.UnregisterInitiator(this);
            return AwaitableUtils.CompletedTask;
        }
    }
}