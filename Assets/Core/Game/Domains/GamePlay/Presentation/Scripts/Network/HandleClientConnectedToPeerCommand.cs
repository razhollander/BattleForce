using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.C2SModels.Packets;
using CoreDomain.Scripts.Services.CommandFactory;
using LiteNetLib;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.Network
{
    public class HandleClientConnectedToPeerCommand : BaseCommand, ICommandVoid
    {
        private IClientNetworkManager _networkManager;
        private readonly JoinRequestPacketC2S _cachedJoinRequest = new();

        public override void ResolveDependencies()
        {
            _networkManager = _diContainer.Resolve<IClientNetworkManager>();
        }

        public void Execute()
        {
            _cachedJoinRequest.UserName = "RazPlayer";
            _networkManager.SendPacketSerialized(PacketTypeC2S.MatchRejoinRequest, _cachedJoinRequest, DeliveryMethod.ReliableOrdered);
        }
    }
}