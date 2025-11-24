using System;
using System.Threading;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager
{
    public class PlaybackTickProcessor : ITickProcessor
    {
        public int CurrentTick { get; private set; }
        private FixedTimer _fixedTimer;

        public void StartTick(int ticksPerSecond, CancellationTokenSource cancellationTokenSource)
        {
            _fixedTimer = new FixedTimer(ticksPerSecond, OnTick);
            _fixedTimer.Start(cancellationTokenSource);
        }

        private void OnTick()
        {
            try
            {
                CurrentTick++;
                //var inputsPerPlayerForCurrentTick = _serverPlayersInputListener.GetSortedInputsPerPlayerForTick(CurrentTick); 
            }
            catch (Exception e)
            {
                LogService.LogError("Got error! " + e.ToString());
                throw;
            }
          
            // Pass inputs to Simulator and update Current State
            //_serverState.Tick = CurrentTick;
            // Send current state to all players
            // _playerManager.LogicUpdate();
            // if (_serverTick % 2 == 0)
            // {
            //     
            //     int pCount = _playerManager.Count;
            //     
            //     foreach(ServerPlayer p in _playerManager)
            //     {
            //         SendStateToPlayer(p, pCount);
            //     }
            // }
        }
        
        public void StopTick()
        {
            _fixedTimer.Stop();
        }
    }
}