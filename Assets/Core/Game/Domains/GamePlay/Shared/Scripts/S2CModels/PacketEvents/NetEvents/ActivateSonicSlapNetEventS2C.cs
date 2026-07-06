using System;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class ActivateSonicSlapNetEventS2C : INetSerializable, IComparable<ActivateSonicSlapNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public FixedUnorderedList<ushort> AffectedPlayerIds;

        public ActivateSonicSlapNetEventS2C(int maxAffectedPlayers)
        {
            AffectedPlayerIds = new FixedUnorderedList<ushort>(maxAffectedPlayers);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)AffectedPlayerIds.Count);
            foreach (var affectedPlayerId in AffectedPlayerIds.AsSpan())
            {
                writer.Put((byte)affectedPlayerId);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            AffectedPlayerIds.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var affectedPlayerId = ref AffectedPlayerIds.AddAndGet();
                affectedPlayerId = reader.GetByte();
            }
        }

        public int CompareTo(ActivateSonicSlapNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
