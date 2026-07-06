namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService
{
    public interface ITickService
    {
        int CurrentTick { get; }
        void StartTick(float speedMultiplier = 1);
        void StopTick();
        void SetSpeedMultiplier(float speedMultiplier);
        void RegisterObserver(ITickObserver observer);
        void UnregisterObserver(ITickObserver observer);
        void SetCurrentTick(int initialTick);
    }
}