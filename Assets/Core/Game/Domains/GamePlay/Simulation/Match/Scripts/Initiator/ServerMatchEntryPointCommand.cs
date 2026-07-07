using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;
using System;
using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayerLockOnTarget;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.ClientsNetworkDataService;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator
{
    public class ServerMatchEntryPointCommand : BaseCommand,ICommandVoid
    {
        private IMatchPlayerJoinPacketsHandler _matchPlayerJoinPacketsHandler;
        private ITickProcessor _tickProcessor;
        private IMatchPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private IMatchDataService _matchDataService;
        private IPlaybackRecorderService _playbackRecorderService;
        private IPlayersTalentsManager _playersTalentsManager;
        private IPlayersPowerUpsManager _playersPowerUpsManager;
        private IServerNetworkManager _networkManager;
        private ISimulationGamePlayConfigService _gamePlayConfigService;
        private ITickService _tickService;
        private ICommandFactory _commandFactory;
        private NetworkConfig _networkConfig;
        private IStageDataService _stageDataService;
        private ISimulationInputService _simulationInputService;
        private ILockOnTargetTimerService _lockOnTargetTimerService;
        private SetRandomTalentsForPlayerCommand _setRandomTalentsForPlayerCommand;
        private IClientsNetworkDataService _clientsNetworkDataService;
        
        private SimulationMatchEnterData _simulationMatchEnterData;

        public ServerMatchEntryPointCommand SetMatchEnterData(SimulationMatchEnterData simulationMatchEnterData)
        {
            _simulationMatchEnterData = simulationMatchEnterData;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchPlayerJoinPacketsHandler = _diContainer.Resolve<IMatchPlayerJoinPacketsHandler>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _playerInputsPacketsHandler = _diContainer.Resolve<IMatchPlayerInputsPacketsHandler>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playbackRecorderService = _diContainer.Resolve<IPlaybackRecorderService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _playersPowerUpsManager = _diContainer.Resolve<IPlayersPowerUpsManager>();
            _networkManager = _diContainer.Resolve<IServerNetworkManager>();
            _gamePlayConfigService = _diContainer.Resolve<ISimulationGamePlayConfigService>();
            _tickService = _diContainer.Resolve<ITickService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _simulationInputService = _diContainer.Resolve<ISimulationInputService>();
            _lockOnTargetTimerService = _diContainer.Resolve<ILockOnTargetTimerService>();
            _clientsNetworkDataService = _diContainer.Resolve<IClientsNetworkDataService>();
            _setRandomTalentsForPlayerCommand =  _commandFactory.CreateCommandVoid<SetRandomTalentsForPlayerCommand>();
        }

        public void Execute()
        {
            InitRNG();
            SetCurrentTickIfPlayback();
            _matchPlayerJoinPacketsHandler.InitEntryPoint();
            TrySwitchToPlayback();

            InitPlayers(_simulationMatchEnterData);
            _playerInputsPacketsHandler.InitEntryPoint();
            _stageDataService.InitEntryPoint();
            _commandFactory.CreateCommandVoid<InitStageCommand>().Execute();
            _tickProcessor.InitEntryPoint();
        }

        private void InitRNG()
        {
            if (_playbackRecorderService.IsPlaybackEnabled)
            {
                RNG.Init(_playbackRecorderService.Seed);
            }
            else
            {
                var rnd = new Random();
                var seed = rnd.Next();
                RNG.Init(seed);
            }
        }

        private void SetCurrentTickIfPlayback()
        {
            if (_playbackRecorderService.IsPlaybackEnabled)
            {
                _tickService.SetCurrentTick(_playbackRecorderService.InitialTick);
            }
        }

        private void TrySwitchToPlayback()
        {
            if (_playbackRecorderService.IsPlaybackEnabled)
            {
                _networkManager.SwitchToNetManager(new NetManagerPlayback(_playbackRecorderService, _tickService, _networkConfig));
            }
            else
            {
                var allPlayers = new List<EnterMatchPlayerData>();
                foreach (var kvp in _simulationMatchEnterData.PlayersPerClient)
                {
                    allPlayers.AddRange(kvp.Value);
                }
                
                _playbackRecorderService.StartRecording(RNG.Seed, allPlayers.ToArray());
            }
        }

        private void InitPlayers(SimulationMatchEnterData simulationMatchEnterData)
        {
            foreach (var kvp in simulationMatchEnterData.PlayersPerClient)
            {
                var clientId = kvp.Key;
                var playerDatas = kvp.Value;

                var didReachByEnteringStraightToMatchThroughCheats = !_clientsNetworkDataService.IsClientExist(clientId);
                if (didReachByEnteringStraightToMatchThroughCheats)
                {
                    _clientsNetworkDataService.AddClient(clientId, false);
                }
                
                for (var i = 0; i < playerDatas.Length; i++)
                {
                    var player = playerDatas[i];
                    var playerId = player.Id;
                    var playerName = player.Name;
                    var playerTeamId = player.TeamId;

                    var startingDirection = Vector2.UnitX;
                    var velocity = Vector2.Zero;
                    var radius = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
                    var health = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.StartHealth;
                    var shootCooldown = _gamePlayConfigService.GamePlayConfig.PlayerSpaceship.ShootCooldown;
                    var position = Vector2.Zero;

                    var didAddPlayerByEnteringStraightToMatchThroughCheats = !_clientsNetworkDataService.IsPlayerAssignedToClient(clientId, playerId);
                    if (didAddPlayerByEnteringStraightToMatchThroughCheats)
                    {
                        _clientsNetworkDataService.AssignPlayerToClient(clientId, playerId);
                    }
                    
                    _matchDataService.AddPlayer(playerId, playerTeamId, playerName, position, startingDirection, velocity, radius, health, shootCooldown);
                    _playersTalentsManager.AddPlayer(playerId);
                    _playersPowerUpsManager.AddPlayer(playerId);
                    _simulationInputService.AddPlayer(playerId);
                    _lockOnTargetTimerService.AddPlayer(playerId);
                    _playersTalentsManager.TryAddTalentToPlayer(TalentType.FrigidBlock, playerId, 0, out _, out _);
                    _playersTalentsManager.TryAddTalentToPlayer(TalentType.GrapplingHook, playerId, 0, out _, out _);
                    _playersTalentsManager.TryAddTalentToPlayer(TalentType.SentryGun, playerId, 0, out _, out _);
                
                    if (_gamePlayConfigService.GamePlayConfig.ShouldChooseRandomTalentsForPlayer)
                    {
                        _setRandomTalentsForPlayerCommand.SetPlayerId(playerId).SetTalentsAmount(_gamePlayConfigService.GamePlayConfig.RandomTalentsForPlayersAmount).Execute();
                    }
                }
            }
        }
    }
}