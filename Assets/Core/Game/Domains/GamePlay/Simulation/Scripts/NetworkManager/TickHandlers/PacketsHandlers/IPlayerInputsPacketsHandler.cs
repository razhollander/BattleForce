using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.PacketsHandlers
{
    public interface IPlayerInputsPacketsHandler
    {
        void RegisterListeners();
        Dictionary<int, PlayerInputPacketC2S> ProcessInputs();
        void InitExitPoint();
    }
}