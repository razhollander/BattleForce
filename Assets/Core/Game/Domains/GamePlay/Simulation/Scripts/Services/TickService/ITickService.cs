namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService
{
    public interface ITickService
    {
        int CurrentTick { get; }
        void InitEntryPoint();
        void InitExitPoint();
        void RegisterObserver(ITickObserver observer);
        void UnregisterObserver(ITickObserver observer);
        void SetCurrentTick(int initialTick);
    }
}