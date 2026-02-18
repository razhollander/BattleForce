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
        public FixedOrderedList<TalentStateS2C> PlayerTalents;
        public bool DidReplaceTalent;

        public TalentCardObtainedNetEventS2C(int maxTalentsPerPlayerAmount)
        {
            PlayerTalents = new FixedOrderedList<TalentStateS2C>(maxTalentsPerPlayerAmount);
        }

        public TalentCardObtainedNetEventS2C()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(TalentCardId);
            writer.Put(ObtainedByPlayerId);
            writer.Put(DidReplaceTalent);
            writer.Put((byte)PlayerTalents.Count);

            foreach (var talent in PlayerTalents.AsSpan())
            {
                talent.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            TalentCardId = reader.GetUShort();
            ObtainedByPlayerId = reader.GetUShort();
            DidReplaceTalent = reader.GetBool();
            var talentsCount = reader.GetByte();
            PlayerTalents.Clear();

            for(int i = 0; i < talentsCount; i++)
            {
                ref var talent = ref PlayerTalents.AddAndGet();
                talent.Deserialize(reader);
            }
        }
    }
}