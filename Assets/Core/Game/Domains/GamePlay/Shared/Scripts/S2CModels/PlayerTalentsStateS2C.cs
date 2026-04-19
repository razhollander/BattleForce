using System.Numerics;
using Core.Scripts.Utils.CustomCollections;
using Core.Game.Domains.GamePlay.Shared.Extensions;
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
        
        public bool TrySetIsTalentActive(TalentType talentType, bool isActive)
        {
            if (Talents.Count == 0)
            {
                return false;
            }
            
            for (int i = 0; i < Talents.Count; i++)
            {
                var talent = Talents[i];
                if (talent.TalentType == talentType)
                {
                    ref var talentState = ref Talents.Get(i);
                    talentState.IsActive = isActive;
                    return true;
                }
            }
            
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

        public void CopyFrom(PlayerTalentsStateS2C other)
        {
            this.SelectedTalentIndex = other.SelectedTalentIndex;
            this.AimDirection = other.AimDirection;
            this.Talents.Clear();
            
            for (int i = 0; i < other.Talents.Count; i++)
            {
                ref var talentState = ref this.Talents.AddAndGet();
                talentState = other.Talents[i];
            }
        }
    }

    public struct TalentStateS2C
    {
        public TalentType TalentType;
        public TalentCooldownType CooldownType;
        public TalentNormalCooldownStateS2C NormalCooldown;
        public TalentStocksCooldownStateS2C StocksCooldown;
        public bool IsActive;
        
        public bool IsOnCooldown() => 
            CooldownType == TalentCooldownType.Normal ? NormalCooldown.IsOnCooldown() : (CooldownType == TalentCooldownType.Stocks ? StocksCooldown.IsOnCooldown() : false);

        public void Setup(TalentType talentType)
        {
            TalentType = talentType;
        }

        public void SetupWithNormalCooldown(float maxCooldown)
        {
            CooldownType = TalentCooldownType.Normal;
            NormalCooldown.MaxCooldown = maxCooldown;
            NormalCooldown.CooldownEndTick = 0;
            StocksCooldown = default;
        }
        

        public void SetupWithAlwaysActiveCooldown()
        {
            CooldownType = TalentCooldownType.AlwaysActive;
            NormalCooldown = default;
            StocksCooldown = default;
        }

        public void SetupWithStocksCooldown(int maxStocksAmount, float singleStockCooldown)
        {
            CooldownType = TalentCooldownType.Stocks;
            StocksCooldown.MaxStocksAmount = maxStocksAmount;
            StocksCooldown.CurrentStocksAmount = maxStocksAmount;
            StocksCooldown.MaxSingleStockCooldown = singleStockCooldown;
            StocksCooldown.RecieveNextStockOnTick = 0;
            NormalCooldown = default;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)TalentType);
            writer.Put((byte)CooldownType);

            switch (CooldownType)
            {
                case TalentCooldownType.Stocks: StocksCooldown.Serialize(writer); break;
                case TalentCooldownType.Normal: NormalCooldown.Serialize(writer); break;
                case TalentCooldownType.AlwaysActive: break;
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            TalentType = (TalentType)reader.GetByte();
            CooldownType = (TalentCooldownType)reader.GetByte();
            
            switch (CooldownType)
            {
                case TalentCooldownType.Stocks: StocksCooldown.Deserialize(reader); break;
                case TalentCooldownType.Normal: NormalCooldown.Deserialize(reader); break;
                case TalentCooldownType.AlwaysActive: break;
            }
        }

        public void ClearCooldown()
        {
            switch (CooldownType)
            {
                case TalentCooldownType.Stocks: StocksCooldown.ClearCooldown(); break;
                case TalentCooldownType.Normal: NormalCooldown.ClearCooldown(); break;
                case TalentCooldownType.AlwaysActive: break;
            }
        }
    }
    
    public struct TalentNormalCooldownStateS2C
    {
        private const int NO_COOLDOWN_TICK = 0;

        public int CooldownEndTick; 
        public float MaxCooldown;

        public bool IsOnCooldown() => CooldownEndTick > NO_COOLDOWN_TICK;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CooldownEndTick);
            writer.PutFloat16(MaxCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            CooldownEndTick = reader.GetInt();
            MaxCooldown = reader.GetFloat16();
        }

        public void ClearCooldown() => CooldownEndTick = NO_COOLDOWN_TICK;
    }
    
    public struct TalentStocksCooldownStateS2C
    {
        public int CurrentStocksAmount;
        public int MaxStocksAmount;
        public int RecieveNextStockOnTick;
        public float MaxSingleStockCooldown;
        public bool IsOnCooldown() => CurrentStocksAmount == 0;
        public bool IsAtMaxStocks() => CurrentStocksAmount == MaxStocksAmount;
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)CurrentStocksAmount);
            writer.Put((byte)MaxStocksAmount);
            writer.Put(RecieveNextStockOnTick);
            writer.Put(MaxSingleStockCooldown);
        }

        public void Deserialize(NetDataReader reader)
        {
            CurrentStocksAmount = reader.GetByte();
            MaxStocksAmount = reader.GetByte();
            RecieveNextStockOnTick = reader.GetInt();
            MaxSingleStockCooldown = reader.GetFloat();
        }

        public void ClearCooldown()
        {
            CurrentStocksAmount = MaxStocksAmount;
            RecieveNextStockOnTick = 0;
        }
    }
}