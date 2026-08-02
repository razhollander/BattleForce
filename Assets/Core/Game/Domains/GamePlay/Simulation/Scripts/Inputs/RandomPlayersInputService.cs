using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    // Testing only: fabricates "dumb player" input so a match can be driven without real clients.
    // Enabled via SimulationGamePlayInnerConfig.RandomPlayersInput. Each player keeps a small
    // "intent" (move direction, aim, shoot) that is re-rolled every few ticks so the movement
    // looks like an aimless human rather than per-tick jitter, while talents and power-ups are
    // occasionally tapped at random.
    public class RandomPlayersInputService : IRandomPlayersInputService
    {
        private const int MinTicksBetweenDecisions = 15;
        private const int MaxTicksBetweenDecisions = 45;
        private const float ShootChance = 0.85f;
        private const float TalentPressChancePerTick = 0.01f;
        private const float PowerUpPressChancePerTick = 0.005f;
        private const float BarrelDashPressChancePerTick = 0.01f;

        private readonly CapacityDict<ushort, RandomInputState> _statePerPlayer;

        public RandomPlayersInputService(NetworkConfig networkConfig)
        {
            _statePerPlayer = new CapacityDict<ushort, RandomInputState>(networkConfig.MaxCap.ConcurrentPlayers);
        }

        public void ApplyRandomInput(ref MatchLocalPlayerInputDataC2S input)
        {
            if (!_statePerPlayer.TryGetValue(input.PlayerId, out var state))
            {
                state = new RandomInputState();
                _statePerPlayer.Add(input.PlayerId, state);
                RerollDecision(state);
            }

            state.TicksUntilNextDecision--;
            if (state.TicksUntilNextDecision <= 0)
            {
                RerollDecision(state);
            }

            input.IsMoveLeftInputPressed = state.MoveDirection < 0;
            input.IsMoveRightInputPressed = state.MoveDirection > 0;
            input.IsUsingMouseAim = false;
            input.MouseWorldPosition = Vector2.Zero;
            input.AimDirection = state.AimDirection;
            input.IsShootInputPressed = state.IsShooting;
            
            input.IsTalentAInputPressed = RNG.RNG.NextFloat() < TalentPressChancePerTick;
            input.IsTalentBInputPressed = RNG.RNG.NextFloat() < TalentPressChancePerTick;
            input.IsTalentCInputPressed = RNG.RNG.NextFloat() < TalentPressChancePerTick;
            input.IsPowerUpInputPressed = RNG.RNG.NextFloat() < PowerUpPressChancePerTick;
            input.IsBarrelDashInputPressed = RNG.RNG.NextFloat() < BarrelDashPressChancePerTick;
        }

        private void RerollDecision(RandomInputState state)
        {
            state.TicksUntilNextDecision = RNG.RNG.NextInt(MinTicksBetweenDecisions, MaxTicksBetweenDecisions);
            state.MoveDirection = RNG.RNG.NextInt(-1, 2); // -1, 0 or 1
            state.AimDirection = RNG.RNG.NextFloat(0f, 360f).AngleToVector();
            state.IsShooting = RNG.RNG.NextFloat() < ShootChance;
        }

        private class RandomInputState
        {
            public int TicksUntilNextDecision;
            public int MoveDirection;
            public Vector2 AimDirection;
            public bool IsShooting;
        }
    }
}
