using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public interface IRandomPlayersInputService
    {
        // Overwrites the input's movement/aim/shoot/talent/power-up fields with fabricated
        // "dumb player" input. PlayerId is left untouched.
        void ApplyRandomInput(ref MatchLocalPlayerInputDataC2S input);
    }
}
