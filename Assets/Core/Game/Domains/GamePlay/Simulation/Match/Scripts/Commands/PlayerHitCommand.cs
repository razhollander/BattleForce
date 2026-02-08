using System;
using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using Utils;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class PlayerHitCommand : BaseCommand, ICommandVoid
    {
        private const ushort DEAD_HEALTH_AMOUNT = 0;
        private const ushort GEMS_COLLECTED_FOR_TEAM_ALIVE = 1;
        
        private ushort _hitDamage;
        private ushort _playerId;
        private IMatchDataService _matchDataService;
        private INetEventsDataService _netEventsDataService;
        private ICommandFactory _commandFactory;
        private IStageDataService _stageDataService;
        private SimulationGamePlayConfig _gamePlayConfig;
        private int _processedTick;
        private HashSet<ushort> _chachedTeamsCurrentlyAlive;
        private SharedGamePlayConfig _sharedGamePlayConfig;
        private Dictionary<ushort,Dictionary<ushort, int>> _gemsGainedPerTeamIdPerTeam;
        private Dictionary<ushort,Dictionary<ushort, int>> _totalGemsPerTeamIdPerTeam;
        private NetworkConfig _networkConfig;

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
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
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
            var playerState = _matchDataService.SimulationState.GetPlayerById(_playerId);

            if (!playerState.Spaceship.IsAlive)
            {
                return;
            }
            
            var newHealth = (ushort)Math.Max(DEAD_HEALTH_AMOUNT, playerState.Spaceship.Health.CurrentHealth - _hitDamage);
            playerState.Spaceship.Health.CurrentHealth = newHealth;
            var isPlayerAlive = newHealth > DEAD_HEALTH_AMOUNT;

          
            LogService.LogTopic($"Player Hit! Id {_playerId} hit with damage {_hitDamage}, new health: {newHealth}, is alive: {isPlayerAlive}", LogTopicType.ServerNetwork);
            _netEventsDataService.AddPlayerTakeDamageNetEvent(_processedTick, _playerId, newHealth, _hitDamage, isPlayerAlive);

            if (!isPlayerAlive)
            {
                KillPlayer(playerState);
            }
        }

        private void KillPlayer(PlayerStateS2C playerState)
        {
            playerState.Spaceship.IsAlive = false;
            var shootState = playerState.Spaceship.Shoot;
            var isShootOnCooldown = shootState.MaxCooldown > shootState.CooldownSecondsLeft; 
            shootState.MaxCooldown *= _gamePlayConfig.ShootCooldownMultiplierWhenDead;

            if (!isShootOnCooldown)
            {
                shootState.CooldownSecondsLeft = shootState.MaxCooldown;
            }
            
            playerState.Spaceship.Shoot = shootState;
            playerState.Spaceship.IsEngineOn = false;
            playerState.Spaceship.Transform.Velocity = Vector2.Zero;
            _netEventsDataService.AddPlayerDiedNetEvent(_processedTick, _playerId, shootState.MaxCooldown, shootState.CooldownSecondsLeft);

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
            
            foreach (ushort teamAlive in teamsCurrentlyAlive)
            {
                gemsGainedPerTeam.Add(teamAlive, GEMS_COLLECTED_FOR_TEAM_ALIVE);
                totalGemsPerTeam.Add(teamAlive, ++_matchDataService.SimulationState.GemsPerTeamId[teamAlive]);
                _stageDataService.AddGemsForTeam(teamAlive, GEMS_COLLECTED_FOR_TEAM_ALIVE);
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
