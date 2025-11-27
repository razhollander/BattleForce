namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public interface ITickProcessor
    {
        int CurrentTick { get; }
        void StopTick();
        void InitEntryPoint();
    }
}