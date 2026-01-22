using Core.Scripts.Utils.CustomCollections;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class PlayerTalentsStateS2C
    {
        public int SelectedTalentIndex;
        public FixedOrderedList<TalentStateS2C> Talents;

        public ref TalentStateS2C GetCurrentSelectedTalent()
        {
            return ref Talents.Get(SelectedTalentIndex);
        }

        public PlayerTalentsStateS2C(int maxTalents)
        {
            Talents = new FixedOrderedList<TalentStateS2C>(maxTalents);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)SelectedTalentIndex);
            writer.Put((byte)Talents.Count);

            foreach (var talent in Talents.AsSpan())
            {
                talent.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            SelectedTalentIndex = reader.GetByte();
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
        public bool IsOnCooldown() => CooldownSecondsLeft < MaxCooldown;

        public TalentStateS2C(TalentType talentType, float cooldownSecondsLeft, float maxCooldown)
        {
            TalentType = talentType;
            CooldownSecondsLeft = cooldownSecondsLeft;
            MaxCooldown = maxCooldown;
        }

        public void Setup(TalentType talentType, float maxCooldown)
        {
            TalentType = talentType;
            CooldownSecondsLeft = maxCooldown;
            MaxCooldown = maxCooldown;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)TalentType);
            writer.PutFloat16(CooldownSecondsLeft);
            writer.PutFloat16(MaxCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            TalentType = (TalentType)reader.GetByte();
            CooldownSecondsLeft = reader.GetFloat16();
            MaxCooldown = reader.GetFloat16();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            writer.PutFloat16(CooldownSecondsLeft);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            CooldownSecondsLeft = reader.GetFloat16();
        }
    }
}