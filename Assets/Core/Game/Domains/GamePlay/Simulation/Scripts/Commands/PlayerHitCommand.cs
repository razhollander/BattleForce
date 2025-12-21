using System;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class PlayerHitCommand : BaseCommand, ICommandVoid
    {
        private const int DEAD_HEALTH_AMOUNT = 0;
        
        private int _hitDamage;
        private ushort _playerId;
        private IMatchDataService _matchDataService;

        public PlayerHitCommand SetHitDamage(int hitDamage)
        {
            _hitDamage = hitDamage;

            return this;
        }

        public PlayerHitCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;

            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService =_diContainer.Resolve<IMatchDataService>();
        }

        public void Execute()
        {
            var playerState = _matchDataService.GetPlayer(_playerId);
            playerState.Spaceship.Health.CurrentHealth = Math.Max(DEAD_HEALTH_AMOUNT, playerState.Spaceship.Health.CurrentHealth - _hitDamage);
            var isPlayerDead = playerState.Spaceship.Health.CurrentHealth == DEAD_HEALTH_AMOUNT;

            if (isPlayerDead)
            {
                playerState.IsAlive = false;
            }
            
            _matchDataService.SetPlayer(playerState.Id, playerState);
            // Creating and dispatching PlayerHitEvent
            var playerHitEvent = new PlayerHitEvent(_playerId, _hitDamage);
            _diContainer.Resolve<IEventSystem>().Dispatch(playerHitEvent);
        }
    }
}