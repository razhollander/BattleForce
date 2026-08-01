using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public interface IRandomPlayersInputService
    {
        void ApplyRandomInput(ref MatchLocalPlayerInputDataC2S input);
    }
}
