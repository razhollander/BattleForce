using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface INetManagerWrapper
    {
        //bool IsRunning { get; }
        void SetPacketsListener(NetworkC2SPacketsListener packetsListener);
        void Start(int port);
        void Stop();
        void PollEvents();
        int ConnectedPeersCount { get; }
    }
}