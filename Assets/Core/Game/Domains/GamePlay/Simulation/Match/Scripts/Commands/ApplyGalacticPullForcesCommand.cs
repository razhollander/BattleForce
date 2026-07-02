using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class ApplyGalacticPullForcesCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private NetworkConfig _networkConfig;
        private ICommandFactory _commandFactory;
        private AddForceToPlayerCommand _addForceToPlayerCommand;

        private int _tick;

        public ApplyGalacticPullForcesCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _addForceToPlayerCommand = _commandFactory.CreateCommandVoid<AddForceToPlayerCommand>();
        }

        public void Execute()
        {
            var fields = _matchDataService.SimulationState.GalacticForceFields;
            if (fields.Count == 0)
            {
                return;
            }

            var pullForce = _gamePlayConfigService.GamePlayConfig.PowerUps.GalacticPullForce;
            var deltaTime = _networkConfig.DeltaTime;
            var pullDelta = new Vector2(0f, -pullForce * deltaTime);

            for (int i = fields.Count - 1; i >= 0; i--)
            {
                var field = fields[i];

                if (_tick >= field.EndTick)
                {
                    _matchDataService.SimulationState.RemoveGalacticForceFieldById(field.Id);
                    _netEventsDataService.AddDeactivateGalacticForceFieldNetEvent(_tick, field.Id);
                    continue;
                }

                foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
                {
                    var isAllyPlayer = playerState.TeamId == field.CasterTeamId;
                    if (isAllyPlayer)
                    {
                        continue;
                    }

                    _addForceToPlayerCommand.SetPlayerId(playerState.Id).SetForce(pullDelta).Execute();
                }
            }
        }
    }
}
