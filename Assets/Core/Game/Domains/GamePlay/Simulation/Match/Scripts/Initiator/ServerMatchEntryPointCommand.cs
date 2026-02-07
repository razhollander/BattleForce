using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator
{
    public class ServerMatchEntryPointCommand : BaseCommand,ICommandVoid
    {
        private IMatchPlayerJoinPacketsHandler _matchPlayerJoinPacketsHandler;
        private ITickProcessor _tickProcessor;
        private IMatchPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private IPhysicsSimulator _physicsSimulator;
        private SimulationGamePlayConfig _simulationGamePlayConfig;
        private IMatchDataService _matchDataService;
        private IPlaybackRecorderService _playbackRecorderService;
        private IPlayersTalentsManager _playersTalentsManager;
        private IServerNetworkManager _networkManager;
        private SimulationGamePlayConfig _gamePlayConfig;
        private INetEventsDataService _netEventsDataService;
        private ITickService _tickService;
        private ICommandFactory _commandFactory;
        
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
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationGamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playbackRecorderService = _diContainer.Resolve<IPlaybackRecorderService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _networkManager = _diContainer.Resolve<IServerNetworkManager>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _netEventsDataService = _diContainer.Resolve<INetEventsDataService>();
            _tickService = _diContainer.Resolve<ITickService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
        }

        public void Execute()
        {
            InitRNG();
            InitTickService();
            _matchDataService.InitEntryPoint();
            _playerInputsPacketsHandler.InitEntryPoint();
            _matchPlayerJoinPacketsHandler.InitEntryPoint();

            InitPlayers(_simulationMatchEnterData);
            _commandFactory.CreateCommandVoid<InitStageCommand>().Execute();

            TrySwitchToPlayback();
            
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

        private void InitTickService()
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
                _networkManager.SwitchToNetManager(new NetManagerPlayback(_playbackRecorderService, _tickService));
            }
            else
            {
                _playbackRecorderService.StartRecording(RNG.Seed, _simulationMatchEnterData.Players);
            }
        }

        private void InitPlayers(SimulationMatchEnterData simulationMatchEnterData)
        {
            var playerDatas = simulationMatchEnterData.Players;

            for (var i = 0; i < playerDatas.Length; i++)
            {
                var player = playerDatas[i];
                var playerId = player.Id;
                var playerName = player.Name;
                var playerTeamId = player.TeamId;
                
                var startingDirection = Vector2.UnitX;
                var velocity = Vector2.Zero;
                var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
                var health = _gamePlayConfig.PlayerSpaceship.StartHealth;
                var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
                var position = Vector2.Zero;
                var isPlayerConnected = _networkManager.IsPlayerPeerConencted(playerId);
                _matchDataService.AddPlayer(playerId, playerTeamId, playerName, position, startingDirection, velocity, radius, health, shootCooldown, isPlayerConnected);
                _playersTalentsManager.AddPlayer(playerId);
            }
        }
    }
}