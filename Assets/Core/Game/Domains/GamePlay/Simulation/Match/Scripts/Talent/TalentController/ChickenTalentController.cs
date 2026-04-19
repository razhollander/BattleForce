using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class ChickenTalentController : ITalentController
    {
        public bool IsCurrentlyActive { get; private set; }

        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private ushort _casterPlayerId;
        private readonly NetworkConfig _networkConfig;
        private readonly IPhysicsSimulator _physicsSimulator;

        private int _countdownEndTick;

        public ChickenTalentController(IMatchDataService matchDataService,
            INetEventsDataService netEventsDataService,
            SimulationGamePlayConfig gamePlayConfig,
            NetworkConfig networkConfig, IPhysicsSimulator physicsSimulator)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfig = gamePlayConfig;
            _physicsSimulator = physicsSimulator;
            _networkConfig = networkConfig;
        }

        public void SetCasterId(ushort id) { _casterPlayerId = id; }

        public void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed, int tick, float deltaTime)
        {
        }

        public void StopIfActive(int tick)
        {
            if (IsCurrentlyActive)
            {
                DeactivateTalent(tick);
            }
        }

        public void OnTick(int tick, float deltaTime)
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);

            bool isSelected = false;
            if (casterPlayerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent))
            {
                isSelected = selectedTalent.TalentType == TalentType.Chicken;
            }

            if (isSelected)
            {
                if (!IsCurrentlyActive)
                {
                    ActivateTalent(tick);
                }
                else
                {
                    if (tick >= _countdownEndTick)
                    {
                        var config = _gamePlayConfig.Talents.ChickenTalentConfig;

                        var aimDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
                        if (aimDirection == Vector2.Zero) aimDirection = casterPlayerState.Spaceship.Transform.Direction;
                        if (aimDirection == Vector2.Zero) aimDirection = new Vector2(1, 0);

                        casterPlayerState.Spaceship.Transform.Velocity += aimDirection * config.PushForce;

                        var egg = _matchDataService.AddChickenEgg(_casterPlayerId, casterPlayerState.Spaceship.Transform.Position);
                        _physicsSimulator.AddChickenEgg(egg.Id, casterPlayerState.TeamId, egg.Position, casterPlayerState.Spaceship.Transform.Radius);

                        _netEventsDataService.AddLayChickenEggNetEventS2C(tick, _casterPlayerId, egg.Id, egg.Position);

                        _countdownEndTick = TickUtils.GetTickPassedAfterDuration(tick, config.CountdownDuration, _networkConfig.DeltaTime);
                    }
                }
            }
            else
            {
                if (IsCurrentlyActive)
                {
                    DeactivateTalent(tick);
                }
            }
        }

        private void ActivateTalent(int tick)
        {
            IsCurrentlyActive = true;
            var config = _gamePlayConfig.Talents.ChickenTalentConfig;
            _countdownEndTick = TickUtils.GetTickPassedAfterDuration(tick, config.CountdownDuration, _networkConfig.DeltaTime);
            _netEventsDataService.AddActivateChickenTalentNetEventS2C(tick, _casterPlayerId);
        }

        private void DeactivateTalent(int tick)
        {
            IsCurrentlyActive = false;
            _netEventsDataService.AddDeactivateChickenTalentNetEventS2C(tick, _casterPlayerId, tick);
        }

        public void ResetData()
        {
            IsCurrentlyActive = false;
        }
    }
}
