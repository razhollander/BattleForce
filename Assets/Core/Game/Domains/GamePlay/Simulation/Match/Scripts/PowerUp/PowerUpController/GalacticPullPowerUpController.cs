using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.Utils;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public class GalacticPullPowerUpController : IPowerUpController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;
        private readonly NetworkConfig _networkConfig;
        private ushort _casterPlayerId;

        public PowerUpType PowerUpType => PowerUpType.GalacticPull;

        public GalacticPullPowerUpController(IMatchDataService matchDataService, INetEventsDataService netEventsDataService,
            ISimulationGamePlayConfigService gamePlayConfigService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
            _gamePlayConfigService = gamePlayConfigService;
            _networkConfig = networkConfig;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void OnTick(int tick) { }

        public void Perform(int tick)
        {
            var casterTeamId = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId).TeamId;
            CreateGalacticForceFieldForTeam(casterTeamId, tick);
        }

        private void CreateGalacticForceFieldForTeam(ushort casterTeamId, int tick)
        {
            var durationSeconds = _gamePlayConfigService.GamePlayConfig.PowerUps.GalacticPullDurationSeconds;
            var endTick = TickUtils.GetTickPassedAfterDuration(tick, durationSeconds, _networkConfig.DeltaTime);
            var field = _matchDataService.AddGalacticForceField(casterTeamId, endTick);
            _netEventsDataService.AddPerformGalacticPullNetEvent(tick, field.Id, _casterPlayerId, casterTeamId);
        }
    }
}
