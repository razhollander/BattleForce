using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IPlayerInputsPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        CapacityDict<long, int> ProcessInputs(int processedTick);
        void InitExitPoint();
    }
}