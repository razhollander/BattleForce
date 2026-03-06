using System;
using System.Collections.Generic;
using System.Threading;
using Core.Game.Domains.GamePlay.Shared;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService
{
    public class TickService : ITickService
    {
        private readonly NetworkConfig _networkConfig;
        private TimerFixedThreaded2 _fixedTimer;
        private readonly List<ITickObserver> _observers;

        public int CurrentTick { get; private set; }

        public TickService(NetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            _observers = new List<ITickObserver>(2);
        }
        
        public void StartTick()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _fixedTimer = new TimerFixedThreaded2("Server Thread", _networkConfig.TicksPerSeconds, OnTick);
            _fixedTimer.Start(cancellationTokenSource);
        }

        private void OnTick()
        {
            try
            {
                if (_observers.Count == 0)
                {
                    LogService.LogError("No observers registered!");
                }
                
                for (int i = _observers.Count - 1; i >= 0; i--)
                {
                    _observers[i].OnTick(CurrentTick);
                }
                
                CurrentTick++;
            }
            catch (Exception e)
            {
                LogService.LogError("Got error in server thread! " + e);
                StopTick();
                throw;
            }
        }

        public void StopTick()
        {
            _fixedTimer.Stop();
        }

        public void RegisterObserver(ITickObserver observer)
        {
            _observers.Add(observer);
        }

        public void UnregisterObserver(ITickObserver observer)
        {
            _observers.Remove(observer);
        }

        public void SetCurrentTick(int initialTick)
        {
            CurrentTick = initialTick;
        }
    }
}