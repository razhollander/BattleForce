namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService
{
    public interface ITickObserver
    {
        void OnTick(int currentTick);
    }
}