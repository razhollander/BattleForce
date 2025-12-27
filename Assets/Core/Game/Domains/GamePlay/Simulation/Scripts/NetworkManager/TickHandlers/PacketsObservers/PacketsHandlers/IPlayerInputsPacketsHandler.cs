using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers
{
    public interface IPlayerInputsPacketsHandler
    {
        void InitEntryPoint();
        Dictionary<ushort, PlayerInputPacketC2S> ProcessInputs(int processedTick);
        void InitExitPoint();
    }
}