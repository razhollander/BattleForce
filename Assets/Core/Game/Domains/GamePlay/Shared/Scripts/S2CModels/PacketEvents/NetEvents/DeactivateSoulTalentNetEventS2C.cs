using System;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct DeactivateSoulTalentNetEventS2C : INetSerializable, IComparable<DeactivateSoulTalentNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort GhostId;
        public ushort CasterPlayerId;
        public int TalentCooldownEndTick;
        public bool DidTeleport;
        public Vector2 TeleportPosition;
        public Vector2 TeleportDirection;

        public DeactivateSoulTalentNetEventS2C(int occuredOnTick, ushort ghostId, ushort casterPlayerId, int talentCooldownEndTick, bool didTeleport, Vector2 teleportPosition,
            Vector2 teleportDirection)
        {
            OccuredOnTick = occuredOnTick;
            GhostId = ghostId;
            CasterPlayerId = casterPlayerId;
            TalentCooldownEndTick = talentCooldownEndTick;
            DidTeleport = didTeleport;
            TeleportPosition = teleportPosition;
            TeleportDirection = teleportDirection;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)GhostId);
            writer.Put((byte)CasterPlayerId);
            writer.Put(TalentCooldownEndTick);
            writer.Put(DidTeleport);
            writer.PutVector2Quantized(TeleportPosition);
            writer.PutVector2AsAngle16(TeleportDirection);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            GhostId = reader.GetByte();
            CasterPlayerId = reader.GetByte();
            TalentCooldownEndTick = reader.GetInt();
            DidTeleport = reader.GetBool();
            TeleportPosition = reader.GetVector2Quantized();
            TeleportDirection = reader.GetVector2FromAngle16();
        }

        public int CompareTo(DeactivateSoulTalentNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
