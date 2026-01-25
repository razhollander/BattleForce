namespace Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors
{
    public interface ITickProcessor
    {
        void StopTick();
        void InitEntryPoint();
    }
}