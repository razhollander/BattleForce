namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc
{
    public interface IBackgroundParallaxController
    {
        void InitEntryPoint();
        void ManagedUpdate();
        void InitExitPoint();
    }
}