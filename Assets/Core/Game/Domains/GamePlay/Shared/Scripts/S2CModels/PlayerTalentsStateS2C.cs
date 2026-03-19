using System.Numerics;
using Core.Scripts.Utils.CustomCollections;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class PlayerTalentsStateS2C
    {
        public int SelectedTalentIndex;
        public Vector2 AimDirection;
        public FixedOrderedList<TalentStateS2C> Talents;

        public PlayerTalentsStateS2C(int maxTalents)
        {
            Talents = new FixedOrderedList<TalentStateS2C>(maxTalents);
        }

        public ref TalentStateS2C GetCurrentSelectedTalent()
        {
            return ref Talents.Get(SelectedTalentIndex);
        }

        public bool TryGetCurrentSelectedTalent(out TalentStateS2C selectedTalent)
        {
            if (Talents.Count == 0)
            {
                selectedTalent = default;
                return false;
            }
            
            selectedTalent = Talents.Get(SelectedTalentIndex);
            return true;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)SelectedTalentIndex);
            writer.PutVector2AsAngle16(AimDirection);
            writer.Put((byte)Talents.Count);

            foreach (var talent in Talents.AsSpan())
            {
                talent.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            SelectedTalentIndex = reader.GetByte();
            AimDirection = reader.GetVector2FromAngle16();
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
            writer.PutVector2AsAngle16(AimDirection);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            AimDirection = reader.GetVector2FromAngle16();
        }
    }

    public struct TalentStateS2C
    {
        public TalentType TalentType;
        public int CooldownEndTick;
        public float MaxCooldown;
        public bool IsOnCooldown() => CooldownEndTick > 0;

        public TalentStateS2C(TalentType talentType, float maxCooldown)
        {
            TalentType = talentType;
            MaxCooldown = maxCooldown;
            CooldownEndTick = 0;
        }

        public void Setup(TalentType talentType, float maxCooldown)
        {
            TalentType = talentType;
            MaxCooldown = maxCooldown;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)TalentType);
            writer.Put(CooldownEndTick);
            writer.PutFloat16(MaxCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            TalentType = (TalentType)reader.GetByte();
            CooldownEndTick = reader.GetInt();
            MaxCooldown = reader.GetFloat16();
        }




    }
}