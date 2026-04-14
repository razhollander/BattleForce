using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerGrapplingHookHitNetEventS2C : INetSerializable, IComparable<PlayerGrapplingHookHitNetEventS2C>
    {
        public int OccuredOnTick;
        public ushort HookProjectileId;

        public PlayerGrapplingHookHitNetEventS2C(int occuredOnTick, ushort hookProjectileId)
        {
            OccuredOnTick = occuredOnTick;
            HookProjectileId = hookProjectileId;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(HookProjectileId);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            HookProjectileId = reader.GetUShort();
        }

        public int CompareTo(PlayerGrapplingHookHitNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
