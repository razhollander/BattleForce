using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public struct MatchPlayerInputPacketC2S : INetSerializable,IComparable<MatchPlayerInputPacketC2S>
    {
        // todo: add inputs from client unprocessed ticks
        public int Tick;
        public int HeighestProcessedTickFromServer;
        public bool IsMoveRightInputPressed;
        public bool IsMoveLeftInputPressed;
        public bool IsShootInputPressed;
        public bool IsTalentInputPressed;
        public bool IsSwitchTalentInputPressed;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.Put(HeighestProcessedTickFromServer);
            writer.Put(ConvertInputsToByte(IsMoveRightInputPressed, IsMoveLeftInputPressed, IsShootInputPressed, IsTalentInputPressed, IsSwitchTalentInputPressed));
            writer.Put(Tick);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            HeighestProcessedTickFromServer = reader.GetInt();
            ConvertByteToInputs(reader.GetByte(), out var isMoveRightInputPressed, out var isMoveLeftInputPressed, out var isShootInputPressed, out var isTalentInputPressed, out var isSwitchTalentInputPressed);
            IsMoveRightInputPressed = isMoveRightInputPressed;
            IsMoveLeftInputPressed = isMoveLeftInputPressed;
            IsShootInputPressed = isShootInputPressed;
            IsTalentInputPressed = isTalentInputPressed;
            IsSwitchTalentInputPressed = isSwitchTalentInputPressed;
            Tick = reader.GetInt();
        }
        
        private byte ConvertInputsToByte(bool isMoveRightInputPressed, bool isMoveLeftInputPressed, bool isShootInputPressed, bool isTalentInputPressed, bool isSwitchTalentInputPressed)
        {
            return (byte)(
                (isMoveRightInputPressed ? 1 << 0 : 0) |
                (isMoveLeftInputPressed ? 1 << 1 : 0) |
                (isShootInputPressed ? 1 << 2 : 0) |
                (isTalentInputPressed ? 1 << 3 : 0) |
                (isSwitchTalentInputPressed ? 1 << 4 : 0)
            );
        }
        
        private void ConvertByteToInputs(byte data,out bool right,out bool left,out bool shoot, out bool talent, out bool switchTalent)
        {
            right = (data & (1 << 0)) != 0;
            left  = (data & (1 << 1)) != 0;
            shoot = (data & (1 << 2)) != 0;
            talent = (data & (1 << 3)) != 0;
            switchTalent = (data & (1 << 4)) != 0;
        }
        
        public int CompareTo(MatchPlayerInputPacketC2S other)
        {
            return Tick.CompareTo(other.Tick);
        }
    }
}