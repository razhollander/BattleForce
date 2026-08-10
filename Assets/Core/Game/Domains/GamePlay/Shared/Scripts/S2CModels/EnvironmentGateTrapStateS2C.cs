using System;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    /// <summary>
    /// The only gate trap data that is not derivable from the layout config. It rides along the full state snapshot so a
    /// rejoining client can pick the cycle up mid-swing; during the match a single GateTrapClosing net event carries it.
    /// </summary>
    [Serializable]
    public struct EnvironmentGateTrapStateS2C : INetSerializable, IEquatable<ushort>
    {
        public ushort Id;
        public GateTrapState State;

        // Tick the current state ends on. While Open it is the tick the trap may close again on (its cooldown end).
        public int StateEndTick;

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
