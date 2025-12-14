using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public struct PlayerInputPacketC2S : INetSerializable,IComparable<PlayerInputPacketC2S>
    {
        // todo: add inputs from client unprocessed ticks
        public int Tick;
        public int LastProcessedTickFromServer;
        public bool IsMoveRightInputPressed;
        public bool IsMoveLeftInputPressed;
        public bool IsShootInputPressed;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.Put(LastProcessedTickFromServer);
            writer.Put(ConvertInputsToByte(IsMoveRightInputPressed, IsMoveLeftInputPressed, IsShootInputPressed));
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            LastProcessedTickFromServer = reader.GetInt();
            ConvertByteToInputs(reader.GetByte(), out var isMoveRightInputPressed, out var isMoveLeftInputPressed, out var isShootInputPressed);
            IsMoveRightInputPressed = isMoveRightInputPressed;
            IsMoveLeftInputPressed = isMoveLeftInputPressed;
            IsShootInputPressed = isShootInputPressed;

        }
        
        private byte ConvertInputsToByte(bool isMoveRightInputPressed, bool isMoveLeftInputPressed, bool isShootInputPressed)
        {
            return (byte)(
                (isMoveRightInputPressed ? 1 << 0 : 0) |
                (isMoveLeftInputPressed ? 1 << 1 : 0) |
                (isShootInputPressed ? 1 << 2 : 0)
            );
        }
        
        private void ConvertByteToInputs(byte data,out bool right,out bool left,out bool shoot)
        {
            right = (data & (1 << 0)) != 0;
            left  = (data & (1 << 1)) != 0;
            shoot = (data & (1 << 2)) != 0;
        }

        public int CompareTo(PlayerInputPacketC2S other)
        {
            return Tick.CompareTo(other.Tick);
        }
    }
}