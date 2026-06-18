using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IMatchPlayerInputsPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        bool DidReceiveAnyInputFromClient(long clientId);
        ProcessPlayersInputsResult ProcessInputs(int processedTick, float deltaTime);
        void InitExitPoint();
    }
}