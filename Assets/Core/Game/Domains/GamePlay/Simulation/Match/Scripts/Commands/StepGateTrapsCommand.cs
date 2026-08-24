using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class StepGateTrapsCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private int _tick;
        private float _deltaTime;

        public StepGateTrapsCommand SetTime(int tick, float deltaTime)
        {
            _tick = tick;
            _deltaTime = deltaTime;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var environmentData = _matchDataService.EnvironmentData;

            if (environmentData.GateTraps.IsEmpty)
            {
                return;
            }

            var simulationState = _matchDataService.SimulationState;
            var wheelCalculationTick = _tick - simulationState.PreperationPhaseEndedOnTick;

            foreach (var gateTrap in environmentData.GateTraps.AsSpan())
            {
                ref var gateTrapState = ref simulationState.GetGateTrapById(gateTrap.Id);

                EnvironmentGateTrapUtils.AdvanceTimedStates(ref gateTrapState.State, ref gateTrapState.StateEndTick, _tick,
                    gateTrap.TransitionDurationInTicks, gateTrap.StayClosedDurationInTicks, gateTrap.StayOpenDurationInTicks);

                if (EnvironmentGateTrapUtils.CanStartClosing(gateTrapState.State, gateTrapState.StateEndTick, _tick) && IsAnyPlayerInsideArea(gateTrap, wheelCalculationTick))
                {
                    gateTrapState.State = GateTrapState.Closing;
                    gateTrapState.StateEndTick = _tick + gateTrap.TransitionDurationInTicks;
                    _netEventsDataService.AddGateTrapClosingNetEvent(_tick, gateTrap.Id, gateTrapState.StateEndTick);
                }

                UpdateWallTransform(gateTrap, gateTrapState.State, gateTrapState.StateEndTick);
            }
        }

        private void UpdateWallTransform(MatchEnvironmentGateTrapModel gateTrap, GateTrapState state, int stateEndTick)
        {
            var isGatePositionIdle = state is GateTrapState.Closed or GateTrapState.Open;
            if (isGatePositionIdle)
            {
                return;
            }

            var closedProgress = EnvironmentGateTrapUtils.CalculateClosedProgress(state, stateEndTick, _tick, gateTrap.TransitionDurationInTicks);
            EnvironmentGateTrapUtils.CalculateWallTransform(gateTrap.OpenPosition, gateTrap.ClosedPosition, gateTrap.OpenRotationDegrees, gateTrap.ClosedRotationDegrees,
                gateTrap.LocalRotationPivot, closedProgress, out var localPosition, out var localRotationDegrees);

            var wall = _matchDataService.EnvironmentData.GetWall(gateTrap.WallId);
            wall.Transform.LocalPosition = localPosition;
            wall.Transform.LocalRotationDegrees = localRotationDegrees;

            if (gateTrap.IsAttachedToRotationWheel)
            {
                // StepAllWheelsRotationCommand runs right after this and turns the local transform above into the world one.
                return;
            }

            wall.Transform.WorldPosition = localPosition;
            wall.Transform.WorldRotationDegrees = localRotationDegrees;
        }

        private bool IsAnyPlayerInsideArea(MatchEnvironmentGateTrapModel gateTrap, int wheelCalculationTick)
        {
            var wheelRotationDegrees = 0f;
            var wheelCenter = Vector2.Zero;

            if (gateTrap.IsAttachedToRotationWheel)
            {
                var wheel = _matchDataService.EnvironmentData.GetRotatingWheel(gateTrap.AttachedToRotationWheelId);
                wheelCenter = wheel.CenterPosition;
                wheelRotationDegrees = EnvironmentRotatingWheelUtils.CalculateRotationDuringTick(wheelCalculationTick, wheel.RotationSpeed, _deltaTime);
            }

            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!player.Spaceship.IsAlive)
                {
                    continue;
                }

                var position = player.Spaceship.Transform.Position;

                if (gateTrap.IsAttachedToRotationWheel)
                {
                    // The area is authored in wheel-local space, so the player is brought into it instead of rotating every polygon.
                    position = (position - wheelCenter).Rotate(-wheelRotationDegrees);
                }

                if (gateTrap.IsAnyPointInsideArea(position))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
