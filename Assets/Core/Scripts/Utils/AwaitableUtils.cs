using System;
using System.Collections.Generic;
using System.Threading;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core.Scripts.Utils
{
    public static class AwaitableUtils
    {
        private static readonly AwaitableCompletionSource completionSource = new();
	
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
        
        public static void Forget(this Awaitable awaitable)
        {
            _ = ForgetAsync(awaitable);
        }

        private static async Awaitable ForgetAsync(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception e)
            {
                LogService.LogException(e);
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
        
        public static async Awaitable WithCancellation(this AwaitableCompletionSource source, CancellationToken token)
        {
            using (token.Register(() => source.TrySetCanceled()))
            {
                await source.Awaitable;
            }
        }
        
        public static void CancelWhenTokenCancelled(this CancellationTokenSource cancellationTokenSource, CancellationToken token)
        {
            token.Register(cancellationTokenSource.Cancel);
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

        public static async Awaitable WhenAll(this ICollection<Awaitable> awaitables)
        {
            foreach (var awaitable in awaitables)
            {
                await awaitable;
            }
        }
    }
}