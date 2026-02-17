using System;

namespace Core.Scripts.Services.UnityThreadDispatcher
{
    public interface IUnityMainThreadDispatcher
    {
        void InitEntryPoint();
        void Enqueue(Action action);
        [Obsolete("Dont use this! Use EnqueueDraw")]
        void EnqueueDrawInternal(Action action);
    }
}