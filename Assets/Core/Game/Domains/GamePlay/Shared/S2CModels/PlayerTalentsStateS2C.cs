using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct PlayerTalentsStateS2C
    {
        public FixedOrderedList<TalentStateS2C> Talents;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Talents.Count);

            foreach (var talent in Talents.AsSpan())
            {
                talent.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            var talentsCount = (int)reader.GetByte();
            Talents.Clear();

            for (int i = 0; i < talentsCount; i++)
            {
                ref var talent = ref Talents.AddAndGet();
                talent.Deserialize(reader);
            }
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.Put((byte)Talents.Count);

            foreach (var talent in Talents.AsSpan())
            {
                talent.SerializeDeltas(writer);
            }
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            var talentsCount = (int)reader.GetByte();
            Talents.Clear();

            for (int i = 0; i < talentsCount; i++)
            {
                ref var talent = ref Talents.AddAndGet();
                talent.DeserializeDeltas(reader);
            }
        }
    }

    public struct TalentStateS2C
    {
        public TalentType TalentType;
        public float CooldownSecondsLeft;
        public float MaxCooldown;     
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)TalentType);
            writer.Put(CooldownSecondsLeft);
            writer.Put(MaxCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            TalentType = (TalentType)reader.GetByte();
            CooldownSecondsLeft = reader.GetFloat();
            MaxCooldown = reader.GetFloat();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.Put(CooldownSecondsLeft);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            CooldownSecondsLeft = reader.GetFloat();
        }
    }
}