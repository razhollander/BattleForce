using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.NetworkManager.TickHandlers.PacketsObservers;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Playback;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Talent;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator
{
    public class ServerMatchEntryPointCommand : BaseCommand,ICommandVoid
    {
        private IPlayeRejoinPacketsHandler _playeRejoinPacketsHandler;
        private ITickProcessor _tickProcessor;
        private IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
        private IPhysicsSimulator _physicsSimulator;
        private SimulationGamePlayConfig _simulationGamePlayConfig;
        private IMatchDataService _matchDataService;
        private IPlaybackRecorderService _playbackRecorderService;
        private IPlayersTalentsManager _playersTalentsManager;
        private IServerNetworkManager _networkManager;
        private ICommandFactory _commandFactory;
        private SimulationGamePlayConfig _gamePlayConfig;
        
        private SimulationMatchEnterData _simulationMatchEnterData;

        public ServerMatchEntryPointCommand SetMatchEnterData(SimulationMatchEnterData simulationMatchEnterData)
        {
            _simulationMatchEnterData = simulationMatchEnterData;
            return this;
        }

        public override void ResolveDependencies()
        {
            _playeRejoinPacketsHandler = _diContainer.Resolve<IPlayeRejoinPacketsHandler>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _playerInputsPacketsHandler = _diContainer.Resolve<IPlayerInputsPacketsHandler>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _simulationGamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _playbackRecorderService = _diContainer.Resolve<IPlaybackRecorderService>();
            _playersTalentsManager = _diContainer.Resolve<IPlayersTalentsManager>();
            _networkManager = _diContainer.Resolve<IServerNetworkManager>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
        }

        public void Execute()
        {
            InitPlaybackAndRNG();
            _matchDataService.InitEntryPoint();
            _playerInputsPacketsHandler.InitEntryPoint();
            _playeRejoinPacketsHandler.InitEntryPoint();
            _tickProcessor.InitEntryPoint();
            _physicsSimulator.InitEntryPoint();

            InitPlayers(_simulationMatchEnterData);
            CreateWalls();
            CreateLavaWalls();
            CreateTalentCards();
            TrySwitchToPlayback();
        }

        private void TrySwitchToPlayback()
        {
            if (_playbackRecorderService.IsPlaybackEnabled)
            {
                _networkManager.SwitchToNetManager(new NetManagerPlayback(_playbackRecorderService));
            }
        }

        private void InitPlaybackAndRNG()
        {
            _playbackRecorderService.InitEntryPoint();
            
            if (_playbackRecorderService.IsPlaybackEnabled)
            {
                _playbackRecorderService.LoadRecording();
                RNG.Init(_playbackRecorderService.Seed);
            }
            else
            {
                var rnd = new Random();
                var seed = rnd.Next();
                RNG.Init(seed);
                _playbackRecorderService.StartRecording(seed);
            }
        }

        private void InitPlayers(SimulationMatchEnterData simulationMatchEnterData)
        {
            for (var i = 0; i < simulationMatchEnterData.Players.Length; i++)
            {
                var player = simulationMatchEnterData.Players[i];
                var playerId = player.Id;
                var playerName = player.Name;
                var playerTeamId = player.TeamId;
                
                var startingDirection = RNG.NextFloat(0, 360).AngleToVector();
                var velocity = startingDirection * _gamePlayConfig.PlayerSpaceship.MovementSpeed;
                var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;
                var health = _gamePlayConfig.PlayerSpaceship.StartHealth;
                var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
                var position = Vector2.One;
                var playersAmount = _matchDataService.SimulationState.Players.Count;
                var playerColor = _gamePlayConfig.PlayerSpaceship.ColorPerTeamId[playersAmount % _gamePlayConfig.PlayerSpaceship.ColorPerTeamId.Count];
                _matchDataService.AddPlayer(playerId, playerTeamId, playerName, position, startingDirection, velocity, radius, health, shootCooldown, playerColor);
                _physicsSimulator.AddPlayer(playerId, playerTeamId, position, startingDirection, radius);
                _playersTalentsManager.AddPlayer(playerId);
            }
        }

        private void CreateWalls()
        {
            var wallConfigs = _matchDataService.Environment.WallConfigs;

            foreach (var wallConfig in wallConfigs)
            {
                var wallId = wallConfig.Id;
                var wallPoints = wallConfig.Points;
                _physicsSimulator.AddWall(wallId, wallPoints);
            }
        }

        private void CreateLavaWalls()
        {
            var lavaWallConfigs = _matchDataService.Environment.LavaWallConfigs;
            if (lavaWallConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var lavaWallConfig in lavaWallConfigs)
            {
                var lavaWallId = lavaWallConfig.Id;
                var lavaWallPoints = lavaWallConfig.Points;
                _physicsSimulator.AddLavaWall(lavaWallId, lavaWallPoints);
            }
        }

        private void CreateTalentCards()
        {
            var talentCards = _matchDataService.Environment.TalentCards;
            if (talentCards.IsNullOrEmpty())
            {
                return;
            }
            
            foreach (var talentCard in talentCards)
            {
                var talentCardPosition = talentCard.Position;
                var talentCardId = talentCard.Id;
                _matchDataService.AddTalentCard(talentCardId, talentCardPosition, talentCard.TalentType, _simulationGamePlayConfig.Talents.TalentCardHealth);
                _physicsSimulator.AddTalentCard(talentCardId, talentCardPosition, _simulationGamePlayConfig.Talents.TalentCardWidth, _simulationGamePlayConfig.Talents.TalentCardHeight);
            }
        }
    }
}