using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.MatchInitData;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.TickService;
using Core.Scripts.Extensions;
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
        private IServerNetworkManager _networkManager;
        private SimulationGamePlayConfig _gamePlayConfig;
        private ITickService _tickService;
        private ICommandFactory _commandFactory;
        private NetworkConfig _networkConfig;
        private IStageDataService _stageDataService;
        private ISimulationInputService _simulationInputService;
        
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
            _networkManager = _diContainer.Resolve<IServerNetworkManager>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _tickService = _diContainer.Resolve<ITickService>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _simulationInputService = _diContainer.Resolve<ISimulationInputService>();
        }

        public void Execute()
        {
            InitRNG();
            SetCurrentTickIfPlayback();
            _playerInputsPacketsHandler.InitEntryPoint();
            _matchPlayerJoinPacketsHandler.InitEntryPoint();
            TrySwitchToPlayback();

            InitPlayers(_simulationMatchEnterData);
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
                _simulationInputService.AddPlayer(playerId);
                _playersTalentsManager.TryAddTalentToPlayer(TalentType.Swap, playerId, 0, out _, out _);
                _playersTalentsManager.TryAddTalentToPlayer(TalentType.SentryGun, playerId, 0, out _, out _);
                _playersTalentsManager.TryAddTalentToPlayer(TalentType.DashPulse, playerId, 0, out _, out _);
            }
        }
    }
}