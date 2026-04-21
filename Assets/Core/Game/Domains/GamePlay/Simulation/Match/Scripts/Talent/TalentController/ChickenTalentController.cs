using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent.TalentController
{
    public class ChickenTalentController : ITalentController
    {
        public TalentType TalentType => TalentType.Chicken;

        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly SimulationGamePlayConfig _gamePlayConfig;
        private readonly NetworkConfig _networkConfig;
        private readonly IPhysicsSimulator _physicsSimulator;

        private ushort _casterPlayerId;
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
            
        }

        public void OnTick(int tick, float deltaTime)
        {
            var casterPlayerState = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId);
            var isSelected = false;

            if (casterPlayerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var selectedTalent))
            {
                isSelected = selectedTalent.TalentType == TalentType;
            }

            if (!isSelected)
            {
                return;
            }

            var isOnCountdown = tick < _countdownEndTick;
            if (isOnCountdown)
            {
                return;
            }

            var config = _gamePlayConfig.Talents.ChickenTalentConfig;

            var aimDirection = casterPlayerState.Spaceship.TalentsState.AimDirection;
            casterPlayerState.Spaceship.Transform.Velocity += aimDirection * config.PushForce;

            var egg = _matchDataService.AddChickenEgg(_casterPlayerId, casterPlayerState.Spaceship.Transform.Position);
            _physicsSimulator.AddChickenEgg(egg.Id, casterPlayerState.TeamId, egg.Position, casterPlayerState.Spaceship.Transform.Radius);
            _netEventsDataService.AddLayChickenEggNetEventS2C(tick, _casterPlayerId, egg.Id, egg.Position);
            
            _countdownEndTick = TickUtils.GetTickPassedAfterDuration(tick, config.CountdownDuration, _networkConfig.DeltaTime);
        }

        public void ResetData()
        {
            _countdownEndTick = 0;
        }
    }
}
