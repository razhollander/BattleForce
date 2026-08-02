using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System;
using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.OverrideableNetEvents;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class TryHitPlayerCommand : BaseCommand, ICommandVoid
    {
        private const ushort DEAD_HEALTH_AMOUNT = 0;
        
        private ushort _hitDamage;
        private ushort _playerIdGotHit;
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private ICommandFactory _commandFactory;
        private IStageDataService _stageDataService;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private int _processedTick;
        private HashSet<ushort> _chachedTeamsCurrentlyAlive;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private Dictionary<ushort,Dictionary<ushort, int>> _gemsGainedPerTeamIdPerTeam;
        private Dictionary<ushort,Dictionary<ushort, int>> _totalGemsPerTeamIdPerTeam;
        private PlayerGainedBoltsCommand _playerGainedBoltsCommand;
        private ushort _byPlayerId;
        private bool _wasHitByAnotherPlayer;
        private IOverrideableNetEventsService _overrideableNetEventsService;

        public TryHitPlayerCommand SetHitDamage(ushort hitDamage)
        {
            _hitDamage = hitDamage;
            return this;
        }

        public TryHitPlayerCommand SetPlayerIdGotHit(ushort playerIdGotHit)
        {
            _playerIdGotHit = playerIdGotHit;
            return this;
        }
        
        public TryHitPlayerCommand SetWasHitByAnotherPlayer(bool wasHitByAnotherPlayer, ushort byPlayerId = default)
        {
            _byPlayerId = byPlayerId;
            _wasHitByAnotherPlayer = wasHitByAnotherPlayer;
            return this;
        }
        
        public TryHitPlayerCommand SetProcessedTick(int processedTick)
        {
            _processedTick = processedTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService =_diContainer.Resolve<IMatchDataService>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _overrideableNetEventsService = _diContainer.Resolve<IOverrideableNetEventsService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _playerGainedBoltsCommand = _commandFactory.CreateCommandVoid<PlayerGainedBoltsCommand>();
            _chachedTeamsCurrentlyAlive = new HashSet<ushort>(_sharedGamePlayConfig.MaxTeamsAmount);
            _gemsGainedPerTeamIdPerTeam = new Dictionary<ushort, Dictionary<ushort, int>>(_sharedGamePlayConfig.MaxTeamsAmount);

            int teamIdsCount = _matchDataService.TeamIds.Count;
            foreach (ushort teamId in _matchDataService.TeamIds)
            {
                _gemsGainedPerTeamIdPerTeam[teamId] = new Dictionary<ushort, int>(teamIdsCount);
            } 
            
            _totalGemsPerTeamIdPerTeam = new Dictionary<ushort, Dictionary<ushort, int>>(_sharedGamePlayConfig.MaxTeamsAmount);

            foreach (ushort teamId in _matchDataService.TeamIds)
            {
                _totalGemsPerTeamIdPerTeam[teamId] = new Dictionary<ushort, int>(teamIdsCount);
            }
        }

        public void Execute()
        {
            var playerState = _matchDataService.SimulationState.GetPlayerById(_playerIdGotHit);

            if (!playerState.Spaceship.IsAlive)
            {
                return;
            }
            
            var isPlayerInvulnerableToDamage = _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_playerIdGotHit, TalentType.Rock)
                || _matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer(_playerIdGotHit, TalentType.Frozen);
            if (isPlayerInvulnerableToDamage)
            {
                return;
            }
            
            var newHealth = (ushort)Math.Max(DEAD_HEALTH_AMOUNT, playerState.Spaceship.Health.CurrentHealth - _hitDamage);
            playerState.Spaceship.Health.CurrentHealth = newHealth;
            var isStillAlive = newHealth > DEAD_HEALTH_AMOUNT;
            
            LogService.LogTopic($"Player Hit! Id {_playerIdGotHit} hit with damage {_hitDamage}, new health: {newHealth}, is alive: {isStillAlive}", LogTopicType.ServerNetwork);
            _netEventsDataService.AddPlayerTakeDamageNetEvent(_processedTick, _playerIdGotHit, newHealth, _hitDamage, isStillAlive);
            var boltsGained = _gamePlayConfigService.GamePlayConfig.BoltsGainedPerHit;
            
            if (!isStillAlive)
            {
                KillPlayer(playerState);
                boltsGained += _gamePlayConfigService.GamePlayConfig.BoltsGainedPerKill;
            }
            
            if(_wasHitByAnotherPlayer)
            {
                _playerGainedBoltsCommand
                .SetPlayerId(_byPlayerId)
                .SetGainedAmount(boltsGained)
                .SetProcessedTick(_processedTick)
                .Execute();
            }
        }

        private void KillPlayer(PlayerStateS2C playerState)
        {
            playerState.Spaceship.IsAlive = false;
            var shootState = playerState.Spaceship.Shoot;
            var isShootOnCooldown = shootState.MaxCooldown > shootState.CooldownSecondsLeft; 
            shootState.MaxCooldown *= _gamePlayConfigService.GamePlayConfig.ShootCooldownMultiplierWhenDead;

            if (!isShootOnCooldown)
            {
                shootState.CooldownSecondsLeft = shootState.MaxCooldown;
            }
            
            playerState.Spaceship.Shoot = shootState;
            playerState.Spaceship.IsEngineOn = false;
            playerState.Spaceship.Transform.Velocity = Vector2.Zero;
            _netEventsDataService.AddPlayerDiedNetEvent(_processedTick, _playerIdGotHit);
            _overrideableNetEventsService.OverridePlayerMaxShootCooldownChangedEvent(_processedTick, _playerIdGotHit, shootState.MaxCooldown, shootState.CooldownSecondsLeft);

            if (!_stageDataService.IsStageEnded)
            {
                TryAddLosingTeam(playerState.TeamId);
                TryInvokeMatchEnded();
            }
        }

        private void TryAddLosingTeam(ushort losingTeamId)
        {
            if (IsAnyPlayerAliveInTeam(losingTeamId))
            {
                return;
            }

            _stageDataService.AddLosingTeam(losingTeamId);
            var gemsGainedPerTeam = _gemsGainedPerTeamIdPerTeam[losingTeamId];
            gemsGainedPerTeam.Clear();
            var totalGemsPerTeam= _totalGemsPerTeamIdPerTeam[losingTeamId];
            totalGemsPerTeam.Clear();
            var teamsCurrentlyAlive = GetTeamsCurrentlyAlive();
            
            var gemsCollectedForTeamAlive = _gamePlayConfigService.GamePlayConfig.GemsCollectedForTeamAlive;
            foreach (ushort teamAlive in teamsCurrentlyAlive)
            {
                gemsGainedPerTeam.Add(teamAlive, gemsCollectedForTeamAlive);
                _matchDataService.SimulationState.GemsPerTeamId[teamAlive] += gemsCollectedForTeamAlive;
                totalGemsPerTeam.Add(teamAlive, _matchDataService.SimulationState.GemsPerTeamId[teamAlive]);
                _stageDataService.AddGemsForTeam(teamAlive, gemsCollectedForTeamAlive);
            }
            
            _netEventsDataService.AddTeamLostNetEvent(_processedTick, losingTeamId, totalGemsPerTeam, gemsGainedPerTeam);
        }

        private HashSet<ushort> GetTeamsCurrentlyAlive()
        {
            var losingTeams = _stageDataService.LosingTeamIds;
            var allTeamIds = _matchDataService.TeamIds;
            
            _chachedTeamsCurrentlyAlive.Clear();
            foreach (var teamId in allTeamIds)
            {
                _chachedTeamsCurrentlyAlive.Add(teamId);
            }
            
            _chachedTeamsCurrentlyAlive.ExceptWith(losingTeams);
            return _chachedTeamsCurrentlyAlive;
        }

        private bool IsAnyPlayerAliveInTeam(ushort losingTeamId)
        {
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                var isPlayerAliveInTeam = player.TeamId == losingTeamId && player.Spaceship.IsAlive;
                if (isPlayerAliveInTeam)
                {
                    return true;
                }
            }

            return false;
        }

        private void TryInvokeMatchEnded()
        {
            if (!TryFindWinningTeam(out ushort winningTeamId))
            {
                return;
            }

            _commandFactory.CreateCommandVoid<StageEndedCommand>()
                .SetWinningTeamId(winningTeamId)
                .SetProcessedTick(_processedTick)
                .Execute();
        }

        private bool TryFindWinningTeam(out ushort winningTeamId)
        {
            var didFindAliveTeam = false;
            winningTeamId = 0;
            foreach (var player in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (!player.Spaceship.IsAlive)
                {
                    continue;
                }

                if (didFindAliveTeam && player.TeamId != winningTeamId)
                {
                    return false;
                }
                    
                winningTeamId = player.TeamId;
                didFindAliveTeam = true;
            }

            return true;
        }
    }
}
