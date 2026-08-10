using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    /// <summary>
    /// Client-side twin of a gate trap. The geometry and the durations come from the same layout config the server
    /// reads, so a single GateTrapClosing net event is enough to replay the whole cycle here.
    /// </summary>
    public class MatchEnvironmentGateTrapModel
    {
        public readonly ushort Id;
        public readonly Vector2[] WallPoints;
        public readonly Vector2 OpenPosition;
        public readonly Vector2 ClosedPosition;
        public readonly float OpenRotationDegrees;
        public readonly float ClosedRotationDegrees;
        public readonly Vector2 LocalRotationPivot;
        public readonly int TransitionDurationInTicks;
        public readonly int StayClosedDurationInTicks;
        public readonly int StayOpenDurationInTicks;
        public readonly bool IsAttachedToRotationWheel;
        public readonly ushort AttachedToRotationWheelId;

        public GateTrapState State;
        public int StateEndTick;
        public Vector2 WorldPosition;
        public float WorldRotationAngle;

        // Greys the wall out: the trap is open again but still cooling down, so it cannot catch anybody yet.
        public bool IsWaitingForOpenCooldown;

        public MatchEnvironmentGateTrapModel(ushort id, Vector2[] wallPoints, Vector2 openPosition, Vector2 closedPosition, float openRotationDegrees, float closedRotationDegrees,
            Vector2 localRotationPivot, int transitionDurationInTicks, int stayClosedDurationInTicks, int stayOpenDurationInTicks, bool isAttachedToRotationWheel, ushort attachedToRotationWheelId)
        {
            Id = id;
            WallPoints = wallPoints;
            OpenPosition = openPosition;
            ClosedPosition = closedPosition;
            OpenRotationDegrees = openRotationDegrees;
            ClosedRotationDegrees = closedRotationDegrees;
            LocalRotationPivot = localRotationPivot;
            TransitionDurationInTicks = transitionDurationInTicks;
            StayClosedDurationInTicks = stayClosedDurationInTicks;
            StayOpenDurationInTicks = stayOpenDurationInTicks;
            IsAttachedToRotationWheel = isAttachedToRotationWheel;
            AttachedToRotationWheelId = attachedToRotationWheelId;
        }

        /// <summary>
        /// Brings the trap up to the given server tick: it plays out whatever timed transitions it owes and lands its
        /// wall where the server has it. Pass the wheel it rides, or null for a trap that stands on its own.
        /// </summary>
        public void StepToTick(int tick, int wheelCalculationTick, float deltaTime, MatchEnvironmentRotatingWheelModel rotatingWheel)
        {
            EnvironmentGateTrapUtils.AdvanceTimedStates(ref State, ref StateEndTick, tick, TransitionDurationInTicks, StayClosedDurationInTicks, StayOpenDurationInTicks);
            IsWaitingForOpenCooldown = EnvironmentGateTrapUtils.IsWaitingForOpenCooldown(State, StateEndTick, tick);

            var closedProgress = EnvironmentGateTrapUtils.CalculateClosedProgress(State, StateEndTick, tick, TransitionDurationInTicks);
            EnvironmentGateTrapUtils.CalculateWallTransform(OpenPosition, ClosedPosition, OpenRotationDegrees, ClosedRotationDegrees, LocalRotationPivot, closedProgress,
                out var localPosition, out var localRotationDegrees);

            if (rotatingWheel == null)
            {
                WorldPosition = localPosition;
                WorldRotationAngle = localRotationDegrees;
                return;
            }

            EnvironmentRotatingWheelUtils.CalculateChildTransform(
                wheelCalculationTick, rotatingWheel.RotationSpeed, deltaTime, rotatingWheel.CenterPosition, localPosition, localRotationDegrees,
                out var worldPosition, out var worldRotationDegrees
            );

            WorldPosition = worldPosition;
            WorldRotationAngle = worldRotationDegrees;
        }
    }
}
