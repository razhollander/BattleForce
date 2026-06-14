using System;
using LiteNetLib.Utils;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerTalentCooldownMultiplierChangedNetEventS2C : INetSerializable, IComparable<PlayerTalentCooldownMultiplierChangedNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public float AllTalentsCooldownMultiplier;
        public FixedOrderedList<TalentStateS2C> Talents;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)PlayerId);
            writer.PutFloat16(AllTalentsCooldownMultiplier);
            writer.Put((byte)Talents.Count);

            foreach (var talent in Talents.AsSpan())
            {
                talent.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetByte();
            AllTalentsCooldownMultiplier = reader.GetFloat16();

            var talentsCount = reader.GetByte();

            if (Talents == null)
            {
                Talents = new FixedOrderedList<TalentStateS2C>(talentsCount);
            }

            Talents.Clear();

            for (int i = 0; i < talentsCount; i++)
            {
                ref var talent = ref Talents.AddAndGet();
                talent.Deserialize(reader);
            }
        }

        public int CompareTo(PlayerTalentCooldownMultiplierChangedNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
