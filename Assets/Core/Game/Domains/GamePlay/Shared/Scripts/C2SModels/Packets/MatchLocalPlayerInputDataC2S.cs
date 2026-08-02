using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    public struct MatchLocalPlayerInputDataC2S : INetSerializable
    {
        public ushort PlayerId;
        public bool IsMoveRightInputPressed;
        public bool IsMoveLeftInputPressed;
        public bool IsShootInputPressed;
        public bool IsTalentAInputPressed;
        public bool IsTalentBInputPressed;
        public bool IsTalentCInputPressed;
        public bool IsPowerUpInputPressed;
        public bool IsBarrelDashInputPressed;
        public Vector2 AimDirection;
        public bool IsUsingMouseAim;
        public Vector2 MouseWorldPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PlayerId);

            ushort inputBits = (ushort)(
                (IsMoveRightInputPressed  ? 1 << 0 : 0) |
                (IsMoveLeftInputPressed   ? 1 << 1 : 0) |
                (IsShootInputPressed      ? 1 << 2 : 0) |
                (IsTalentAInputPressed    ? 1 << 3 : 0) |
                (IsTalentBInputPressed    ? 1 << 4 : 0) |
                (IsTalentCInputPressed    ? 1 << 5 : 0) |
                (IsPowerUpInputPressed    ? 1 << 6 : 0) |
                (IsUsingMouseAim          ? 1 << 7 : 0) |
                (IsBarrelDashInputPressed ? 1 << 8 : 0)
            );

            writer.Put(inputBits);
            writer.PutVector2AsAngle16(AimDirection);

            if (IsUsingMouseAim)
            {
                writer.PutVector2Quantized(MouseWorldPosition);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetByte();

            ushort inputBits = reader.GetUShort();
            IsMoveRightInputPressed  = (inputBits & (1 << 0)) != 0;
            IsMoveLeftInputPressed   = (inputBits & (1 << 1)) != 0;
            IsShootInputPressed      = (inputBits & (1 << 2)) != 0;
            IsTalentAInputPressed    = (inputBits & (1 << 3)) != 0;
            IsTalentBInputPressed    = (inputBits & (1 << 4)) != 0;
            IsTalentCInputPressed    = (inputBits & (1 << 5)) != 0;
            IsPowerUpInputPressed    = (inputBits & (1 << 6)) != 0;
            IsUsingMouseAim          = (inputBits & (1 << 7)) != 0;
            IsBarrelDashInputPressed = (inputBits & (1 << 8)) != 0;

            AimDirection = reader.GetVector2FromAngle16();
            MouseWorldPosition = IsUsingMouseAim ? reader.GetVector2Quantized() : Vector2.Zero;
        }
    }
}