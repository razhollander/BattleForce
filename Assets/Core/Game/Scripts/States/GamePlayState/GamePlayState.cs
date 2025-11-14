using System.Threading;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.StateMachineService;
using UnityEngine;
using Zenject;

namespace CoreDomain.GameDomain.Scripts.States.GamePlayState
{
    public class GamePlayState : BaseGameState<GamePlayInitiatorEnterData>
    {
        private readonly ISceneLoaderService _sceneLoaderService;

        public override GameStateType GameStateType => GameStateType.GamePlay;
    
        public GamePlayState(ISceneLoaderService sceneLoaderService, GamePlayInitiatorEnterData gamePlayStateEnterData) : base(gamePlayStateEnterData)
        {
            _sceneLoaderService = sceneLoaderService;
        }

        public override async Awaitable LoadState(CancellationTokenSource cancellationTokenSource)
        {
            await base.LoadState(cancellationTokenSource);
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayScene, EnterData, cancellationTokenSource);
            int c = 0;
        }

        public override async Awaitable StartState(CancellationTokenSource cancellationTokenSource)
        {
            await base.StartState(cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayScene, EnterData, cancellationTokenSource);
        }

        public override async Awaitable ExitState(CancellationTokenSource cancellationTokenSource)
        {
            await base.ExitState(cancellationTokenSource);
            await _sceneLoaderService.TryUnloadScene(SceneType.GamePlayScene, cancellationTokenSource);
        }

        public class Factory : PlaceholderFactory<GamePlayInitiatorEnterData, GamePlayState>
        {
        }
    }
}