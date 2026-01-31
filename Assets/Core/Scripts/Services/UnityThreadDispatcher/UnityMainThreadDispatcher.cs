using System;
using System.Collections.Generic;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Scripts.Services.UnityThreadDispatcher
{
    public class UnityMainThreadDispatcher : IUpdatable, IUnityMainThreadDispatcher
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        public UnityMainThreadDispatcher(IUpdateSubscriptionService updateSubscriptionService)
        {
            _updateSubscriptionService = updateSubscriptionService;
        }

        public void InitEntryPoint()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
        }

        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        public void ManagedUpdate()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
}