using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager
{
    public interface ITickProcessor: ITickObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
    }
}