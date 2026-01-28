using System;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
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
        private StartMatchPacketS2C _startMatchPacket;
        private bool _didReceiveStartMatchPacket;
        private bool _didSwitcToMatch;

        public PacketTypeS2C PacketType => PacketTypeS2C.StartMatch;

        public StartMatchPacketHandler(IClientNetworkManager networkManager, IMatchMakingDataService matchMakingDataService, ISceneLoaderService sceneLoaderService, IStateMachineService stateMachineService, NetworkConfig networkConfig, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _networkManager = networkManager;
            _matchMakingDataService = matchMakingDataService;
            _sceneLoaderService = sceneLoaderService;
            _stateMachineService = stateMachineService;
            _startMatchPacket = new StartMatchPacketS2C(networkConfig.MaxCap, sharedGamePlayConfig.MaxConcurrentTalentsForPlayer);
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
            await _sceneLoaderService.TryUnloadScene(SceneType.GamePlayMatchMakingScene, _stateMachineService.CurrentState().CancellationTokenSource);
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayMatchScene, enterData, _stateMachineService.CurrentState().CancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayMatchScene, enterData, _stateMachineService.CurrentState().CancellationTokenSource);
        }

        public void OnPacketReceived(NetDataReader reader)
        {
            _startMatchPacket.Deserialize(reader);
            _didReceiveStartMatchPacket = true;
            LogService.LogTopic("Start Match accepted received", LogTopicType.ClientNetwork);
        }
    }

    public interface IStartMatchPacketHandler : IPacketsObserver
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ProcessStartMatchPacket();
    }
}