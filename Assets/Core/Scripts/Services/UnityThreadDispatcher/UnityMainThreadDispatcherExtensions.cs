using System;
using System.Diagnostics;

namespace Core.Scripts.Services.UnityThreadDispatcher
{
    public static class UnityMainThreadDispatcherExtensions
    {
        [Conditional("PHYSICS_DEBUG_DRAW_ENABLED")]
        public static void EnqueueDraw(this IUnityMainThreadDispatcher unityMainThread, Action method)
        {
            unityMainThread.EnqueueDrawInternal(method);
        }
    }
}