using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Network;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.DataService
{
    /// <summary>
    /// Views chase the newest server state with an exponential decay filter. The filter's target only moves when a
    /// state packet is processed, so when packets arrive in a burst after a gap the target jumps several ticks at
    /// once: with a fixed decay the view lurches to catch up, then coasts to a standstill during the next gap. That
    /// stall/lurch cycle is what reads as movement in small steps on a jittery connection.
    ///
    /// Slowing the filter in proportion to the gap keeps the rendered speed roughly constant across the cycle. The
    /// decay that closes the enlarged error in exactly the time the gap took is:
    ///
    ///     decay = 1 / (gapInSeconds + e^(-baseDecay * gapInSeconds) / baseDecay)
    ///
    /// which yields ~the configured decay for the healthy one-tick-per-tick case, and progressively gentler values as
    /// gaps grow. The cost is visual lag: a lower decay means the view trails the server state further behind, so this
    /// buys jitter tolerance with latency - the same trade a receive-side jitter buffer makes, paid through the filter.
    /// </summary>
    public class InterpolationDecayService : IInterpolationDecayService
    {
        // Past this the connection is stalling rather than jittering, and crawling is worse than snapping.
        private const int MAX_TICKS_ADVANCED_CONSIDERED = 8;

        // Drops in decay are adopted at once so the view slows down before the next gap, while recoveries are eased in
        // over roughly half a second. Otherwise the decay would flap between values on an alternating gap pattern, and
        // a jumping decay is itself a jump in rendered speed.
        private const float RECOVERY_DECAY = 2f;

        private readonly float _baseDecay;
        private readonly float _tickDeltaTime;

        public float CurrentDecay { get; private set; }

        public InterpolationDecayService(PresentationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig)
        {
            _baseDecay = gamePlayConfig.ExponentialDecay;
            _tickDeltaTime = networkConfig.DeltaTime;
            CurrentDecay = _baseDecay;
        }

        public void UpdateDecayBasedOnTicks(int ticksAdvancedSinceLastProcessedState)
        {
            var ticksAdvanced = Mathf.Clamp(ticksAdvancedSinceLastProcessedState, 1, MAX_TICKS_ADVANCED_CONSIDERED);
            var gapInSeconds = ticksAdvanced * _tickDeltaTime;
            var decayForGap = 1f / (gapInSeconds + Mathf.Exp(-_baseDecay * gapInSeconds) / _baseDecay);

            var isConnectionGettingWorse = decayForGap < CurrentDecay;
            CurrentDecay = isConnectionGettingWorse
                ? decayForGap
                : MathUtils.ExpDecay(CurrentDecay, decayForGap, RECOVERY_DECAY, gapInSeconds);
        }
    }
}
