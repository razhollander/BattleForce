using System;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.SceneService;
using CoreDomain.Scripts.Services.StateMachineService;
using CoreDomain.Scripts.Utils;
using LiteNetLib.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers
{
    public class StartMatchPacketHandler : IStartMatchPacketHandler
    {
        private readonly IClientNetworkManager _networkManager;
        private readonly IMatchMakingDataService _matchMakingDataService;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly IStateMachineService _stateMachineService;
        private readonly ILastFullSyncTickDataService _lastFullSyncTickDataService;
        private readonly StartMatchPacketS2C _startMatchPacket;
        private bool _didReceiveStartMatchPacket;
        private bool _didSwitcToMatch;

        public PacketTypeS2C PacketType => PacketTypeS2C.StartMatch;

        public StartMatchPacketHandler(IClientNetworkManager networkManager, IMatchMakingDataService matchMakingDataService, ISceneLoaderService sceneLoaderService, IStateMachineService stateMachineService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig, ILastFullSyncTickDataService lastFullSyncTickDataService)
        {
            _networkManager = networkManager;
            _matchMakingDataService = matchMakingDataService;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _lastFullSyncTickDataService = lastFullSyncTickDataService;
            _startMatchPacket = new StartMatchPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer, sharedGamePlayConfig.MaxTeamsAmount);
        }

        public void InitEntryPoint()
        {
            _networkManager.RegisterPacketsObserver(this);   
        }

        public void InitExitPoint()
        {
            _networkManager.UnregisterPacketsObserver(this);   
        }
        
        public void ProcessStartMatchPacket()
        {
            if (!_didReceiveStartMatchPacket || _didSwitcToMatch)
            {
                return;
            }

            var state = _startMatchPacket.InitialState;
            var enterData = new GamePlayMatchInitiatorEnterData(state, _matchMakingDataService.LocalPlayer.PlayerId);
            SwitchToMatch(enterData).Forget();
        }

        private async Awaitable SwitchToMatch(GamePlayMatchInitiatorEnterData enterData)
        {
            _didSwitcToMatch = true;
            var cancellationTokenSource = _stateMachineService.CurrentState().CancellationTokenSource;
            await _sceneLoaderService.TryUnloadScene(SceneType.GamePlayMatchMakingScene, cancellationTokenSource);
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayMatchScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayMatchScene, enterData, cancellationTokenSource);
        }

        public void OnPacketReceived(NetDataReader reader)
        {
            _startMatchPacket.Deserialize(reader);
            _didReceiveStartMatchPacket = true;
            _lastFullSyncTickDataService.LastFullSyncTick = _startMatchPacket.OccuredOnTick;
            LogService.LogTopic("Start Match accepted received", LogTopicType.ClientNetwork);
        }
    }
}