namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager
{
    public interface ITickProcessor
    {
        int CurrentTick { get; }
        void InitEntryPoint();
        void InitExitPoint();
    }
}