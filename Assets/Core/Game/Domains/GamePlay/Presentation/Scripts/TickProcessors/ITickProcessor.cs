namespace Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors
{
    public interface ITickProcessor
    {
        void SetTick(int tickOnServer);
        int CurrentTick { get; }
        void StopTick();
        void InitEntryPoint();
    }
}