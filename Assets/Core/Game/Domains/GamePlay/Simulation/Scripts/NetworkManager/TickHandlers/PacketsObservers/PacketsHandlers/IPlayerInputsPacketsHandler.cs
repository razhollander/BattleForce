using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers
{
    public interface IPlayerInputsPacketsHandler : IPacketsObserver
    {
        void InitEntryPoint();
        CapacityDict<ushort, PlayerInputPacketC2S> ProcessInputs(int processedTick);
        void InitExitPoint();
    }
}