namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Controllers
{
    public interface IHeadLessQuitterController
    {
        void InitEntryPoint();
        void InitExitPoint();
        void StepTimer(float deltaTime);
        void QuitIfTimeOut();
    }
}