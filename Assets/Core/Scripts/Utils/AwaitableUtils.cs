using System;
using System.Threading;
using CoreDomain.Scripts.Extensions;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreDomain.Scripts.Utils
{
    public static class AwaitableUtils
    {
        static readonly AwaitableCompletionSource completionSource = new();
	
        public static Awaitable CompletedTask
        {
            get
            {
                completionSource.SetResult();
                var awaitable = completionSource.Awaitable;
                completionSource.Reset();
                return awaitable;
            }
        }
        
        public static async Awaitable WaitUntil(Func<bool> condition, CancellationToken cancellationToken)
        {
            while (!condition())
            {
                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }
        
        public static async Awaitable WhenAny(this Awaitable[] tasks, CancellationToken cancellationToken)
        {
            var tasksLength = tasks.Length;
            if (tasks.IsNullOrEmpty())
            {
                return;
            }

            while (true)
            {
                for (int i = 0; i < tasksLength; i++)
                {
                    if (tasks[i].GetAwaiter().IsCompleted)
                    {
                        return;
                    }
                }
                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        public static Awaitable<T> WithCancellation<T>(this AsyncOperationHandle<T> handle, CancellationToken token)
        {
            var awaitable = new AwaitableCompletionSource<T>();

            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    awaitable.SetResult(operation.Result);
                }
                else if (!token.IsCancellationRequested)
                    throw operation.OperationException ?? new Exception("AsyncOperationHandle failed");
            };

            token.Register(() =>
            {
                awaitable.TrySetCanceled();
            });

            return awaitable.Awaitable;
        }

        public static async Awaitable<UnityEngine.Object> WithCancellation(this ResourceRequest request, CancellationToken token)
        {
            var awaitable = new AwaitableCompletionSource<UnityEngine.Object>();

            request.completed += operation =>
            {
                if (operation.isDone && request.asset != null)
                    awaitable.SetResult(request.asset);
                else if (!token.IsCancellationRequested)
                    awaitable.SetException(new Exception("ResourceRequest failed or asset is null"));
            };

            token.Register(() => { awaitable.TrySetCanceled(); });

            return await awaitable.Awaitable;
        }

        public static async Awaitable WhenAll(this Awaitable[] awaitables)
        {
            foreach (var awaitable in awaitables)
            {
                await awaitable;
            }
        }
    }
}