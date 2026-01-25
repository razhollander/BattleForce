// using System;
// using Core.Game.Domains.GamePlay.Shared.NetworkManager;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.TickHandlers.PacketsObservers.PacketsHandlers;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.Playback;
// using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
// using Core.Scripts.Extensions;
// using Core.Scripts.Utils;
// using CoreDomain.Scripts.Services.CommandFactory;
//
// namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Initiator
// {
//     public class ServerMatchEntryPointCommand_OLD: BaseCommand, ICommandVoid
//     {
//         private IServerNetworkManager _serverNetworkManager;
//         private IPlayerJoinPacketsHandler _playerJoinPacketsHandler;
//         private ITickProcessor _tickProcessor;
//         private IPlayerInputsPacketsHandler _playerInputsPacketsHandler;
//         private IPhysicsSimulator _physicsSimulator;
//         private SimulationGamePlayConfig _simulationGamePlayConfig;
//         private IMatchDataService _matchDataService;
//         private IPlaybackRecorderService _playbackRecorderService;
//
//         public override void ResolveDependencies()
//         {
//             _serverNetworkManager = _diContainer.Resolve<IServerNetworkManager>();
//             _playerJoinPacketsHandler = _diContainer.Resolve<IPlayerJoinPacketsHandler>();
//             _tickProcessor = _diContainer.Resolve<ITickProcessor>();
//             _playerInputsPacketsHandler = _diContainer.Resolve<IPlayerInputsPacketsHandler>();
//             _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
//             _simulationGamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
//             _matchDataService = _diContainer.Resolve<IMatchDataService>();
//             _playbackRecorderService = _diContainer.Resolve<IPlaybackRecorderService>();
//         }
//
//         public void Execute()
//         {
//             InitPlaybackAndRNG();
//             _matchDataService.InitEntryPoint();
//             _serverNetworkManager.InitEntryPoint();
//             _playerInputsPacketsHandler.InitEntryPoint();
//             _playerJoinPacketsHandler.InitEntryPoint();
//             _tickProcessor.InitEntryPoint();
//             _physicsSimulator.InitEntryPoint();
//             
//             CreateWalls();
//             CreateLavaWalls();
//             CreateTalentCards();
//         }
//
//         private void InitPlaybackAndRNG()
//         {
//             _playbackRecorderService.InitEntryPoint();
//             
//             if (_playbackRecorderService.IsPlaybackEnabled)
//             {
//                 _playbackRecorderService.LoadRecording();
//                 RNG.Init(_playbackRecorderService.Seed);
//             }
//             else
//             {
//                 var rnd = new Random();
//                 var seed = rnd.Next();
//                 RNG.Init(seed);
//                 _playbackRecorderService.StartRecording(seed);
//             }
//         }
//
//         private void CreateWalls()
//         {
//             var wallConfigs = _matchDataService.Environment.WallConfigs;
//
//             foreach (var wallConfig in wallConfigs)
//             {
//                 var wallId = wallConfig.Id;
//                 var wallPoints = wallConfig.Points;
//                 _physicsSimulator.AddWall(wallId, wallPoints);
//             }
//         }
//
//         private void CreateLavaWalls()
//         {
//             var lavaWallConfigs = _matchDataService.Environment.LavaWallConfigs;
//             if (lavaWallConfigs.IsNullOrEmpty())
//             {
//                 return;
//             }
//
//             foreach (var lavaWallConfig in lavaWallConfigs)
//             {
//                 var lavaWallId = lavaWallConfig.Id;
//                 var lavaWallPoints = lavaWallConfig.Points;
//                 _physicsSimulator.AddLavaWall(lavaWallId, lavaWallPoints);
//             }
//         }
//
//         private void CreateTalentCards()
//         {
//             var talentCards = _matchDataService.Environment.TalentCards;
//             if (talentCards.IsNullOrEmpty())
//             {
//                 return;
//             }
//             
//             foreach (var talentCard in talentCards)
//             {
//                 var talentCardPosition = talentCard.Position;
//                 var talentCardId = talentCard.Id;
//                 _matchDataService.AddTalentCard(talentCardId, talentCardPosition, talentCard.TalentType, _simulationGamePlayConfig.Talents.TalentCardHealth);
//                 _physicsSimulator.AddTalentCard(talentCardId, talentCardPosition, _simulationGamePlayConfig.Talents.TalentCardWidth, _simulationGamePlayConfig.Talents.TalentCardHeight);
//             }
//         }
//     }
// }