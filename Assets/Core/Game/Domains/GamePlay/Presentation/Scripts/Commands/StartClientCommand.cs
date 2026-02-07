using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.SceneService;
using LiteNetLib;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Commands
{
    public class StartClientCommand : BaseCommand, ICommandAsync
    {
        private bool _isHost;
        private bool _isPlaybackEnabled;
        private string _playbackName;
        private string _ipAddress;
        private int _port;
        private string _playerName;
        
        private IClientNetworkManager _clientNetworkManager;
        private IJoinResponsePacketHandler _joinResponsePacketHandler;
        private ISceneLoaderService _sceneLoaderService;

        public StartClientCommand SetIsHost(bool isHost)
        {
            _isHost = isHost;
            return this;
        }

        public StartClientCommand SetIsPlaybackEnabled(bool isPlaybackEnabled)
        {
            _isPlaybackEnabled = isPlaybackEnabled;
            return this;
        }

        public StartClientCommand SetPlaybackName(string playbackName)
        {
            _playbackName = playbackName;
            return this;
        }

        public StartClientCommand SetServerAddress(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            return this;
        }

        public StartClientCommand SetPlayerName(string playerName)
        {
            _playerName = playerName;
            return this;
        }

        public override void ResolveDependencies()
        {
            _clientNetworkManager = _diContainer.Resolve<IClientNetworkManager>();
            _joinResponsePacketHandler = _diContainer.Resolve<IJoinResponsePacketHandler>();
            _sceneLoaderService = _diContainer.Resolve<ISceneLoaderService>();
        }

        public async Awaitable Execute(CancellationTokenSource cancellationTokenSource)
        {
            _clientNetworkManager.ConenctToServerPeer(_ipAddress, _port, _playerName);
            
            while (!_clientNetworkManager.IsPeerConnected)
            {
                _clientNetworkManager.PollEvents();
                await Awaitable.FixedUpdateAsync(cancellationTokenSource.Token);
            }

            var joinRequest = new JoinRequestPacketC2S(_playerName);
            _clientNetworkManager.SendPacketSerialized(PacketTypeC2S.JoinRequest, joinRequest, DeliveryMethod.ReliableOrdered);
            
            while (!_joinResponsePacketHandler.DidReceiveJoinResponse)
            {
                _clientNetworkManager.PollEvents();
                await Awaitable.FixedUpdateAsync(cancellationTokenSource.Token);
            }

            var joinResponse = _joinResponsePacketHandler.JoinResponse;
            if (!joinResponse.IsSuccess)
            {
                _joinResponsePacketHandler.Reset();
                LogService.LogError("Can't join server!");
                return;
            }

            if (joinResponse.IsMatchMaking)
            {
                var enterData = new GamePlayMatchMakingInitiatorEnterData(joinResponse.MatchMakingSimulationState ,_ipAddress, _port, _isHost, joinResponse.OccuredOnTick, joinResponse.LocalPlayerId);
                await LoadMatchMakingScene(enterData, cancellationTokenSource);
            }
            else
            {
                var InitialState = joinResponse.MatchSimulationState;
                var enterData = new GamePlayMatchInitiatorEnterData(InitialState, InitialState.GetPlayerByName(_playerName).Id);
                await LoadMatchScene(enterData, cancellationTokenSource);
            }
        }
        
        private async Awaitable LoadMatchMakingScene(GamePlayMatchMakingInitiatorEnterData enterData, CancellationTokenSource cancellationTokenSource)
        {
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayMatchMakingScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayMatchMakingScene, enterData, cancellationTokenSource);
        }  
        
        private async Awaitable LoadMatchScene(GamePlayMatchInitiatorEnterData enterData, CancellationTokenSource cancellationTokenSource)
        {
            await _sceneLoaderService.TryLoadScene(SceneType.GamePlayMatchScene, enterData, cancellationTokenSource);
            await _sceneLoaderService.StartScene(SceneType.GamePlayMatchScene, enterData, cancellationTokenSource);
        }

    }
}