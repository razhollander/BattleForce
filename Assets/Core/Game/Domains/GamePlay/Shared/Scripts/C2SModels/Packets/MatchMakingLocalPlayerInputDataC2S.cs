using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    /// <summary>
    /// Holds the specific input data for a single local player.
    /// </summary>
    public struct MatchMakingLocalPlayerInputDataC2S : INetSerializable
    {
        public ushort PlayerId;
        public bool IsMoveRightInputPressed;
        public bool IsMoveLeftInputPressed;
        public bool IsShootInputPressed;
        public bool IsMoveForwardInputPressed;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PlayerId);
            
            byte inputByte = (byte)(
                (IsMoveRightInputPressed ? 1 << 0 : 0) |
                (IsMoveLeftInputPressed  ? 1 << 1 : 0) |
                (IsShootInputPressed     ? 1 << 2 : 0) |
                (IsMoveForwardInputPressed ? 1 << 3 : 0)
            );
            
            writer.Put(inputByte);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetByte();
            
            byte data = reader.GetByte();
            IsMoveRightInputPressed = (data & (1 << 0)) != 0;
            IsMoveLeftInputPressed  = (data & (1 << 1)) != 0;
            IsShootInputPressed     = (data & (1 << 2)) != 0;
            IsMoveForwardInputPressed = (data & (1 << 3)) != 0;
        }
    }
}