using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
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
        public bool IsTalentAInputPressed;
        public bool IsTalentBInputPressed;
        public bool IsTalentCInputPressed;
        public Vector2 AimDirection;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.Put(HeighestProcessedTickFromServer);
            writer.Put(ConvertInputsToByte(IsMoveRightInputPressed, IsMoveLeftInputPressed, IsShootInputPressed, IsTalentAInputPressed, IsTalentBInputPressed, IsTalentCInputPressed));
            writer.PutVector2AsAngle16(AimDirection);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            HeighestProcessedTickFromServer = reader.GetInt();
            ConvertByteToInputs(reader.GetByte(), out var isMoveRightInputPressed, out var isMoveLeftInputPressed, out var isShootInputPressed, out var isTalentAInputPressed, out var isTalentBInputPressed, out var isTalentCInputPressed);
            IsMoveRightInputPressed = isMoveRightInputPressed;
            IsMoveLeftInputPressed = isMoveLeftInputPressed;
            IsShootInputPressed = isShootInputPressed;
            IsTalentAInputPressed = isTalentAInputPressed;
            IsTalentBInputPressed = isTalentBInputPressed;
            IsTalentCInputPressed = isTalentCInputPressed;
            AimDirection = reader.GetVector2FromAngle16();
        }
        
        private byte ConvertInputsToByte(bool isMoveRightInputPressed, bool isMoveLeftInputPressed, bool isShootInputPressed, bool isTalentAInputPressed, bool isTalentBInputPressed, bool isTalentCInputPressed)
        {
            return (byte)(
                (isMoveRightInputPressed ? 1 << 0 : 0) |
                (isMoveLeftInputPressed ? 1 << 1 : 0) |
                (isShootInputPressed ? 1 << 2 : 0) |
                (isTalentAInputPressed ? 1 << 3 : 0) |
                (isTalentBInputPressed ? 1 << 4 : 0) |
                (isTalentCInputPressed ? 1 << 5 : 0)
            );
        }
        
        private void ConvertByteToInputs(byte data,out bool right,out bool left,out bool shoot, out bool talentA, out bool talentB, out bool talentC)
        {
            right = (data & (1 << 0)) != 0;
            left  = (data & (1 << 1)) != 0;
            shoot = (data & (1 << 2)) != 0;
            talentA = (data & (1 << 3)) != 0;
            talentB = (data & (1 << 4)) != 0;
            talentC = (data & (1 << 5)) != 0;
        }
        
        public int CompareTo(MatchPlayerInputPacketC2S other)
        {
            return Tick.CompareTo(other.Tick);
        }
    }
}