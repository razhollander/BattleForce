using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct UpdatePlayerTalentStocksNetEventS2C : INetSerializable, IComparable<UpdatePlayerTalentStocksNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort CasterPlayerId;
        public TalentType TalentType;
        public int CurrentStocksAmount;
        public int RecieveNextStockOnTick;

        public UpdatePlayerTalentStocksNetEventS2C(int occuredOnTick, ushort casterPlayerId, TalentType talentType, int currentStocksAmount, int recieveNextStockOnTick)
        {
            OccuredOnTick = occuredOnTick;
            CasterPlayerId = casterPlayerId;
            TalentType = talentType;
            CurrentStocksAmount = currentStocksAmount;
            RecieveNextStockOnTick = recieveNextStockOnTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put((byte)CasterPlayerId);
            writer.Put((byte)TalentType);
            writer.Put((byte)CurrentStocksAmount);
            writer.Put(RecieveNextStockOnTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            CasterPlayerId = reader.GetByte();
            TalentType = (TalentType)reader.GetByte();
            CurrentStocksAmount = reader.GetByte();
            RecieveNextStockOnTick = reader.GetInt();
        }

        public int CompareTo(UpdatePlayerTalentStocksNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
