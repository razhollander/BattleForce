using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class AddForceToPlayerCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPlayersDecelerationLogic _iIIPlayersDecelerationLogic;
        private INetEventsDataService _netEventsDataService;
        
        private Vector2 _force;
        private bool _shouldTurnOffEngine;
        private ushort _playerId;

        public AddForceToPlayerCommand SetForce(Vector2 force)
        {
            _force = force;
            return this;
        }
        
        public AddForceToPlayerCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public AddForceToPlayerCommand ShouldTurnOffEngine(bool shouldTurnOffEngine)
        {
            _shouldTurnOffEngine = shouldTurnOffEngine;
            return this;
        }
        
        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _iIIPlayersDecelerationLogic = _diContainer.Resolve<IPlayersDecelerationLogic>();
            _netEventsDataService  = _diContainer.Resolve<INetEventsDataService>();
        }

        public void Execute()
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(_playerId);
            playerState.Spaceship.Transform.Velocity += _force;

            if (_shouldTurnOffEngine)
            {
                playerState.Spaceship.IsEngineOn = false;
            }
        }
    }
}