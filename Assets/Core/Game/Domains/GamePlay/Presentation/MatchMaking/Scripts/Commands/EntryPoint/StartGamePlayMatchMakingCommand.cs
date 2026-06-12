using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Background.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.StartMatchButton.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.GameInputActions;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Services.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.CommandFactory;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Commands.EntryPoint
{
    public class StartGamePlayMatchMakingCommand : BaseCommand, ICommandAsync
    {
        private GamePlayMatchMakingInitiatorEnterData _enterData;
        private IMatchMakingPlayerControllers _playerControllers;
        private IMatchMakingBulletControllers _bulletControllers;
        private ITickProcessor _tickProcessor;
        private IMatchMakingEnvironmentWallsControllers _environmentWallsControllers;
        private IFullTickPacketsHandler _fullTickPacketsHandler;
        private IMatchMakingEnvironmentTeamFloorControllers _environmentTeamFloorControllers;
        private IStartMatchButtonController _startMatchButtonController;
        private IStartMatchPacketHandler _startMatchPacketHandler;
        private IMatchMakingUiController _matchMakingUiController;
        private ICommandFactory _commandFactory;
        private ITickCounterService _tickCounterService;
        private IClientNetworkManager _networkManager;
        private NetworkConfig _networkConfig;
        private IClientMatchMakingPresentationTickProcessor _clientPresentationTickProcessor;
        private IMatchMakingDataService _matchMakingDataService;
        private IBackgroundParallaxController _backgroundParallaxController;
        private ILocalPlayersDataService _localPlayersDataService;
        private IGameInputActionsController _gameInputActionsController;

        public StartGamePlayMatchMakingCommand SetEnterData(GamePlayMatchMakingInitiatorEnterData enterData)
        {
            _enterData = enterData;
            return this;
        }

        public override void ResolveDependencies()
        {
            _playerControllers = _diContainer.Resolve<IMatchMakingPlayerControllers>();
            _bulletControllers = _diContainer.Resolve<IMatchMakingBulletControllers>();
            _tickProcessor = _diContainer.Resolve<ITickProcessor>();
            _environmentWallsControllers = _diContainer.Resolve<IMatchMakingEnvironmentWallsControllers>();
            _fullTickPacketsHandler = _diContainer.Resolve<IFullTickPacketsHandler>();
            _environmentTeamFloorControllers = _diContainer.Resolve<IMatchMakingEnvironmentTeamFloorControllers>();
            _startMatchButtonController = _diContainer.Resolve<IStartMatchButtonController>();
            _startMatchPacketHandler = _diContainer.Resolve<IStartMatchPacketHandler>();
            _matchMakingUiController = _diContainer.Resolve<IMatchMakingUiController>();
            _commandFactory = _diContainer.Resolve<ICommandFactory>();
            _tickCounterService = _diContainer.Resolve<ITickCounterService>();
            _networkManager = _diContainer.Resolve<IClientNetworkManager>();
            _networkConfig = _diContainer.Resolve<NetworkConfig>();
            _clientPresentationTickProcessor = _diContainer.Resolve<IClientMatchMakingPresentationTickProcessor>();
            _matchMakingDataService = _diContainer.Resolve<IMatchMakingDataService>();
            _backgroundParallaxController = _diContainer.Resolve<IBackgroundParallaxController>();
            _localPlayersDataService = _diContainer.Resolve<ILocalPlayersDataService>();
            _gameInputActionsController = _diContainer.Resolve<IGameInputActionsController>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            _fullTickPacketsHandler.InitEntryPoint();
            _startMatchPacketHandler.InitEntryPoint();
            _playerControllers.InitEntryPoint();
            _bulletControllers.InitEntryPoint();
            _environmentWallsControllers.InitEntryPoint();
            _environmentTeamFloorControllers.InitEntryPoint();
            _startMatchButtonController.InitEntryPoint();
            _matchMakingUiController.InitEntryPoint(_enterData.IPAddress, _enterData.Port, _enterData.IsHost);
            _tickProcessor.InitEntryPoint();
            _backgroundParallaxController.InitEntryPoint();
            AddPlayersDevices();
            
            _commandFactory.CreateCommandVoid<SyncMatchMakingSimulationStateCommand>()
                .SetSimulationState(_enterData.SimulationState)
                .SetStateOccuredOnTick(_enterData.StateOccuredOnTick)
                .Execute();
            _clientPresentationTickProcessor.StartTick();
        }

        private void AddPlayersDevices()
        {
            _localPlayersDataService.SetLocalPlayers(_enterData.PlayerIdToDeviceIdDictionary);

            foreach (var kvp in _localPlayersDataService.GetPlayerIdToDeviceIdDictionary())
            {
                _gameInputActionsController.AddPlayer(kvp.Key,_localPlayersDataService.GetInputDeviceForPlayer(kvp.Key));
            }
        }
    }
}
