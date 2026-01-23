namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface ITickCounterService
    {
        int CurrentTick { get; }
        void IncrementTick();
    }
    
    public class TickCounterService : ITickCounterService
    {
        public int CurrentTick { get; private set; }
        
        public void IncrementTick() => CurrentTick++;
    }
}