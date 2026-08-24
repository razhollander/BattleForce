using System;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    [Serializable]
    public struct EnvironmentGateTrapStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public GateTrapState State;
        public int StateEndTick; // Tick the current state ends on. While Open it is the tick the trap may close again on (its cooldown end).

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Id);
            writer.Put((byte)State);
            writer.Put(StateEndTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            State = (GateTrapState)reader.GetByte();
            StateEndTick = reader.GetInt();
        }

        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }
    }
}
