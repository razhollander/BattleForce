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

        public override void ResolveDependencies()
        {
            _networkManager = _diContainer.Resolve<IClientNetworkManager>();
        }

        public void Execute()
        {
            _networkManager.SendPacketSerialized(PacketTypeC2S.JoinRequest,
                new JoinRequestPacketC2S { UserName = "RazPlayer" }, DeliveryMethod.Unreliable);
        }
    }
}