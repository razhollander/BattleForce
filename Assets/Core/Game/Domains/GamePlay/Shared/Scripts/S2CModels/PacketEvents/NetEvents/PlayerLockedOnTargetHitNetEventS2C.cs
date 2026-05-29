using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerLockedOnTargetHitNetEventS2C : INetSerializable, IEquatable<PlayerLockedOnTargetHitNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerIdHit;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PlayerIdHit);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerIdHit = reader.GetUShort();
        }

        public bool Equals(PlayerLockedOnTargetHitNetEventS2C other)
        {
            return OccuredOnTick == other.OccuredOnTick && PlayerIdHit == other.PlayerIdHit;
        }
    }
}
