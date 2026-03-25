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

        public bool TryGetTalentIndexByType(TalentType talentType, out int talentIndex)
        {
            if (Talents.Count == 0)
            {
                talentIndex = default;
                return false;
            }
            
            for (int i = 0; i < Talents.Count; i++)
            {
                var talent = Talents[i];
                if (talent.TalentType == talentType)
                {
                    talentIndex = i;
                    return true;
                }
            }

            talentIndex = default;
            return false;
        }
        
        public bool TryGetTalentByType(TalentType talentType, out TalentStateS2C talentState)
        {
            if (Talents.Count == 0)
            {
                talentState = default;
                return false;
            }
            
            for (int i = 0; i < Talents.Count; i++)
            {
                var talent = Talents[i];
                if (talent.TalentType == talentType)
                {
                    talentState = Talents.Get(i);
                    return true;
                }
            }

            talentState = default;
            return false;
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
        private const int NO_COOLDOWN_TICK = 0;
        public TalentType TalentType;
        public int CooldownEndTick;
        public float MaxCooldown;
        public bool IsStockable;
        public int CurrentStocksAmount;
        public int MaxStocksAmount;
        public int ReceiveStockOnTick;

        public bool IsOnCooldown() => CooldownEndTick > NO_COOLDOWN_TICK;
        public void ResetCooldownEndTick() => CooldownEndTick = NO_COOLDOWN_TICK;

        public TalentStateS2C(TalentType talentType, float maxCooldown)
        {
            TalentType = talentType;
            MaxCooldown = maxCooldown;
            CooldownEndTick = 0;
            IsStockable = false;
            CurrentStocksAmount = 0;
            MaxStocksAmount = 0;
            ReceiveStockOnTick = 0;
        }

        public void Setup(TalentType talentType, float maxCooldown)
        {
            TalentType = talentType;
            MaxCooldown = maxCooldown;
            IsStockable = false;
            CurrentStocksAmount = 0;
            MaxStocksAmount = 0;
            ReceiveStockOnTick = 0;
        }

        public void SetupStockable(TalentType talentType, float maxCooldown, int maxStocks)
        {
            TalentType = talentType;
            MaxCooldown = maxCooldown;
            IsStockable = true;
            CurrentStocksAmount = maxStocks;
            MaxStocksAmount = maxStocks;
            ReceiveStockOnTick = 0;
            CooldownEndTick = 0;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)TalentType);
            writer.Put(CooldownEndTick);
            writer.PutFloat16(MaxCooldown);
            writer.Put(IsStockable);
            if (IsStockable)
            {
                writer.Put((byte)CurrentStocksAmount);
                writer.Put((byte)MaxStocksAmount);
                writer.Put(ReceiveStockOnTick);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            TalentType = (TalentType)reader.GetByte();
            CooldownEndTick = reader.GetInt();
            MaxCooldown = reader.GetFloat16();
            IsStockable = reader.GetBool();
            if (IsStockable)
            {
                CurrentStocksAmount = reader.GetByte();
                MaxStocksAmount = reader.GetByte();
                ReceiveStockOnTick = reader.GetInt();
            }
            else
            {
                CurrentStocksAmount = 0;
                MaxStocksAmount = 0;
                ReceiveStockOnTick = 0;
            }
        }
    }
}