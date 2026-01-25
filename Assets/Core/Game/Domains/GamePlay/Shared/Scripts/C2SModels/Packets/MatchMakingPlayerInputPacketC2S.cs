using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public struct MatchMakingPlayerInputPacketC2S : INetSerializable,IComparable<MatchMakingPlayerInputPacketC2S>
    {
        // todo: add inputs from client unprocessed ticks
        public int Tick;
        public int HeighestProcessedTickFromServer;
        public bool IsMoveRightInputPressed;
        public bool IsMoveLeftInputPressed;
        public bool IsShootInputPressed;
        public bool IsMoveForwardInputPressed;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.Put(HeighestProcessedTickFromServer);
            writer.Put(ConvertInputsToByte(IsMoveRightInputPressed, IsMoveLeftInputPressed, IsShootInputPressed, IsMoveForwardInputPressed));
            writer.Put(Tick);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            HeighestProcessedTickFromServer = reader.GetInt();
            ConvertByteToInputs(reader.GetByte(), out var isMoveRightInputPressed, out var isMoveLeftInputPressed, out var isShootInputPressed, out var isMoveForwardInputPressed);
            IsMoveRightInputPressed = isMoveRightInputPressed;
            IsMoveLeftInputPressed = isMoveLeftInputPressed;
            IsShootInputPressed = isShootInputPressed;
            IsMoveForwardInputPressed = isMoveForwardInputPressed;
            Tick = reader.GetInt();
        }
        
        private byte ConvertInputsToByte(bool isMoveRightInputPressed, bool isMoveLeftInputPressed, bool isShootInputPressed, bool isMoveForwardInputPressed)
        {
            return (byte)(
                (isMoveRightInputPressed ? 1 << 0 : 0) |
                (isMoveLeftInputPressed ? 1 << 1 : 0) |
                (isShootInputPressed ? 1 << 2 : 0) |
                (isMoveForwardInputPressed ? 1 << 3 : 0)
            );
        }
        
        private void ConvertByteToInputs(byte data,out bool right,out bool left,out bool shoot, out bool forward)
        {
            right = (data & (1 << 0)) != 0;
            left  = (data & (1 << 1)) != 0;
            shoot = (data & (1 << 2)) != 0;
            forward = (data & (1 << 3)) != 0;
        }
        
        public int CompareTo(MatchMakingPlayerInputPacketC2S other)
        {
            return Tick.CompareTo(other.Tick);
        }
    }
}