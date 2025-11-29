namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation
{
    public interface IClientPresentationTickProcessor
    {
        void StartTick();
        void StopTick();
    }
}