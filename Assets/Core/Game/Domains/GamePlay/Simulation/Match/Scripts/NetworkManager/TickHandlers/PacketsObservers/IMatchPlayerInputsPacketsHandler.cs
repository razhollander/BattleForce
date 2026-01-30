using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers
{
    public interface IMatchPlayerInputsPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        bool DidReceiveAnyInputFromPlayer(ushort playerId);
        ProcessPlayersInputsResult ProcessInputs(int processedTick);
        void InitExitPoint();
    }
}