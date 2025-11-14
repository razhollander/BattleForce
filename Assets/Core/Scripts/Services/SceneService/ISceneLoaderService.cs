using System.Threading;
using CoreDomain.Scripts.CoreInitiator.Base;
using UnityEngine;

namespace CoreDomain.Scripts.Services.SceneService
{
    public interface ISceneLoaderService
    {
        void InitEntryPoint();
        Awaitable<bool> TryLoadScene<TEnterData>(SceneType sceneType, TEnterData enterData, CancellationTokenSource cancellationTokenSource) where TEnterData : class, IInitiatorEnterData;
        Awaitable StartScene<TEnterData>(SceneType gamePlayScene, TEnterData enterData, CancellationTokenSource cancellationTokenSource) where TEnterData : class, IInitiatorEnterData;
        Awaitable<bool> TryUnloadScene(SceneType sceneType, CancellationTokenSource cancellationTokenSource);
    }
}