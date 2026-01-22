namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface INetManagerWrapper
    {
        bool IsRunning { get; }
        void Start(int port);
        void Stop();
        void PollEvents();
    }
}