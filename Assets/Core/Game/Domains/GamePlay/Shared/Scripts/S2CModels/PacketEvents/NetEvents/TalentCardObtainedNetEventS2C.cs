using System.Numerics;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public class TalentCardObtainedNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort TalentCardId;
        public ushort ObtainedByPlayerId;
        public FixedOrderedList<TalentStateS2C> Talents;

        public TalentCardObtainedNetEventS2C(int maxTalentsPerPlayerAmount)
        {
            Talents = new FixedOrderedList<TalentStateS2C>(maxTalentsPerPlayerAmount);
        }

        public TalentCardObtainedNetEventS2C()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(TalentCardId);
            writer.Put(ObtainedByPlayerId);
            writer.Put((byte)Talents.Count);

            foreach (var talent in Talents.AsSpan())
            {
                talent.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            TalentCardId = reader.GetUShort();
            ObtainedByPlayerId = reader.GetUShort();
            var talentsCount = reader.GetByte();
            Talents.Clear();

            for(int i = 0; i < talentsCount; i++)
            {
                ref var talent = ref Talents.AddAndGet();
                talent.Deserialize(reader);
            }
        }
    }
}