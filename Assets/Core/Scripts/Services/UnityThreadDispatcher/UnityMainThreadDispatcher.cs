using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoreDomain.Scripts.Services.UpdateService;

namespace Core.Scripts.Services.UnityThreadDispatcher
{
    public class UnityMainThreadDispatcher : IUpdatable, IGUIUpdatable, IUnityMainThreadDispatcher
    {
        private readonly IUpdateSubscriptionService _updateSubscriptionService;
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        private static readonly Queue<Action> _executionDrawQueue = new Queue<Action>();

        public UnityMainThreadDispatcher(IUpdateSubscriptionService updateSubscriptionService)
        {
            _updateSubscriptionService = updateSubscriptionService;
        }

        public void InitEntryPoint()
        {
            _updateSubscriptionService.RegisterUpdatable(this);
            _updateSubscriptionService.RegisterGuiUpdatable(this);
        }

        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
        
        public void EnqueueDrawInternal(Action action)
        {
            lock (_executionQueue)
            {
                _executionDrawQueue.Enqueue(action);
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

        public void ManagedOnGUI()
        {
            
        }

        public void ManagedOnDrawGizmos()
        {
            lock (_executionDrawQueue)
            {
                while (_executionDrawQueue.Count > 0)
                {
                    _executionDrawQueue.Dequeue().Invoke();
                }
            }
        }
    }
}