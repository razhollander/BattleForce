using System;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Presentation;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.ServerToClientModels;
using Core.Game.Domains.GamePlay.Simulation.NetworkManager.PacketsHandlers;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers
{
    // public class PlayerJoinPacketsHandler : IPlayerJoinPacketsHandler
    // {
        // private readonly IClientNetworkManager _networkManager;
        // private readonly IMatchDataService _matchDataService;
        // private readonly IPlayerControllers _playerControllers;
        // private readonly ITickProcessor _tickProcessor;
        // private readonly NetworkConfig _networkConfig;
        // private readonly IClientPresentationTickProcessor _clientPresentationTickProcessor;
        //
        // public PlayerJoinPacketsHandler(IClientNetworkManager networkManager, IMatchDataService matchDataService,
        //     IPlayerControllers playerControllers, ITickProcessor tickProcessor, NetworkConfig networkConfig, IClientPresentationTickProcessor clientPresentationTickProcessor)
        // {
        //     _networkManager = networkManager;
        //     _matchDataService = matchDataService;
        //     _playerControllers = playerControllers;
        //     _tickProcessor = tickProcessor;
        //     _networkConfig = networkConfig;
        //     _clientPresentationTickProcessor = clientPresentationTickProcessor;
        // }
        //
        // public void RegisterListeners()
        // {
        //     _networkManager.SubscribeNetSerializable<PlayerJoinAcceptPacketS2C, NetPeer>(OnJoinAccept);
        // }
        //
        // public void InitExitPoint()
        // {
        //     UnregisterListeners();
        // }
        //
        // private void UnregisterListeners()
        // {
        //     _networkManager.RemoveSubscription<PlayerJoinAcceptPacketS2C>();
        // }
        //
        // private void OnJoinAccept(PlayerJoinAcceptPacketS2C playerJoinPacketS2C, NetPeer _) // needed netPeer?
        // {
        //     var playerId = playerJoinPacketS2C.PlayerId;
        //     
        //     LogService.LogTopic("Join packet accepted received, player id: " + playerId, LogTopicType.ClientNetwork);
        //     var ticksPassedSinceServerSendPacket = (_networkManager.Ping / 1000f) / _networkConfig.DeltaTime;
        //     var tickWouldBeOnServerWhenReceiveMyPackets = (int)(ticksPassedSinceServerSendPacket*2) + playerJoinPacketS2C.OccuredOnTick;
        //     _tickProcessor.SetTick(tickWouldBeOnServerWhenReceiveMyPackets);
        //     var playerModel = _matchDataService.AddPlayer(playerId, playerJoinPacketS2C.PlayerName, playerJoinPacketS2C.SpaceshipState);
        //     _matchDataService.SetLocalPlayer(playerModel);
        //     _playerControllers.CreatePlayer(playerModel.PlayerId);
        //     _clientPresentationTickProcessor.StartTick();
        // }
    //}
}