using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.PlayersForcesService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class AddForceToPlayerCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        
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
        }

        public void Execute()
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(_playerId);
            if(playerState.Spaceship.TalentsState.TryGetCurrentSelectedTalent(out var currentTalent) && currentTalent.IsCurrentlyActive && 
               currentTalent.TalentType == TalentType.SentryGun)
            {
                return;
            }
            playerState.Spaceship.Transform.Velocity += _force;
            
            if (_shouldTurnOffEngine)
            {
                playerState.Spaceship.IsEngineOn = false;
            }
        }
    }
}