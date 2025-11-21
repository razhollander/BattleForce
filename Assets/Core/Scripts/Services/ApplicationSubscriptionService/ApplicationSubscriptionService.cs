using System.Collections.Generic;
using UnityEngine;

namespace Core.Scripts.Services.ApplicationSubscriptionService
{
    public class ApplicationSubscriptionService : MonoBehaviour, IApplicationSubscriptionService
    {
        private readonly List<IApplicationObserver> _observers = new();
        private void OnApplicationQuit()
        {
            var count = _observers.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                _observers[i].OnApplicationQuit();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            var count = _observers.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                _observers[i].OnApplicationFocus(hasFocus);
            }
        }

        public void RegisterObserver(IApplicationObserver applicationObserver)
        {
            _observers.Add(applicationObserver);
        }

        public void UnregisterObserver(IApplicationObserver applicationObserver)
        {
            _observers.Remove(applicationObserver);
        }
    }
}