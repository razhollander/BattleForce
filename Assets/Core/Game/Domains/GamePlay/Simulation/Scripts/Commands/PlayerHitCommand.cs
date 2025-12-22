using System;
using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Commands
{
    public class PlayerHitCommand : BaseCommand, ICommandVoid
    {
        private const ushort DEAD_HEALTH_AMOUNT = 0;
        
        private ushort _hitDamage;
        private ushort _playerId;
        private IMatchDataService _matchDataService;
        private IMatchNetEventsDataService _matchNetEventsDataService;
        private int _processedTick;

        public PlayerHitCommand SetHitDamage(ushort hitDamage)
        {
            _hitDamage = hitDamage;

            return this;
        }

        public PlayerHitCommand SetPlayerId(ushort playerId)
        {
            _playerId = playerId;
            return this;
        }
        
        public PlayerHitCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService =_diContainer.Resolve<IMatchDataService>();
            _matchNetEventsDataService = _diContainer.Resolve<IMatchNetEventsDataService>();
        }

        public void Execute()
        {
            var playerState = _matchDataService.GetPlayer(_playerId);
            var newHealth = (ushort)Math.Max(DEAD_HEALTH_AMOUNT, playerState.Spaceship.Health.CurrentHealth - _hitDamage);
            playerState.Spaceship.Health.CurrentHealth = newHealth;
            var isPlayerAlive = newHealth > DEAD_HEALTH_AMOUNT;

            if (!isPlayerAlive)
            {
                playerState.IsAlive = false;
            }
            Debug.Log($"Player Hit! Id {_playerId} hit with damage {_hitDamage}, new health: {newHealth}, is alive: {isPlayerAlive}");
            _matchDataService.SetPlayer(playerState.Id, playerState);
            _matchNetEventsDataService.AddPlayerTakeDamageNetEvent(_processedTick, _playerId, newHealth, _hitDamage, isPlayerAlive);
        }
    }
}