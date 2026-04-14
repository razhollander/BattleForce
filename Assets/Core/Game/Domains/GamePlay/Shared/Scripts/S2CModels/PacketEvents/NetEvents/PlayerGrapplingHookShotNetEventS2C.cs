using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerGrapplingHookShotNetEventS2C : INetSerializable, IComparable<PlayerGrapplingHookShotNetEventS2C>
    {
        public int OccuredOnTick;
        public TalentGrapplingHookProjectileS2C HookProjectile;

        public PlayerGrapplingHookShotNetEventS2C(int occuredOnTick, TalentGrapplingHookProjectileS2C hookProjectile)
        {
            OccuredOnTick = occuredOnTick;
            HookProjectile = hookProjectile;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(HookProjectile);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            HookProjectile.Deserialize(reader);
        }

        public int CompareTo(PlayerGrapplingHookShotNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
