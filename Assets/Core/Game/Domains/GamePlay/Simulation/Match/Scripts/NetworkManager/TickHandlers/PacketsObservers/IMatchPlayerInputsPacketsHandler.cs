using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IMatchPlayerInputsPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        bool DidReceiveAnyInputFromClient(long clientId);
        CapacityDict<long, int> ProcessInputs(int processedTick, float deltaTime);
        void InitExitPoint();
    }
}