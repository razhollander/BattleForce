namespace Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors
{
    public class TickCounterService : ITickCounterService
    {
        public int CurrentClientTick { get; private set; }
        
        public void IncrementTick() => CurrentClientTick++;
        public void SetTick(int tick)
        {
            CurrentClientTick = tick;
        }
    }

    public interface ITickCounterService
    {
        int CurrentClientTick { get; }
        void IncrementTick();
        void SetTick(int tick);
    }
}