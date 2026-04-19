using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateGrapplingHookProjectileNetEventS2C : INetSerializable, IComparable<CreateGrapplingHookProjectileNetEventS2C>
    {
        public int OccuredOnTick;
        public TalentGrapplingHookProjectileStateS2C GrapplingHookProjectile;

        public CreateGrapplingHookProjectileNetEventS2C(int occuredOnTick, TalentGrapplingHookProjectileStateS2C projectile)
        {
            OccuredOnTick = occuredOnTick;
            GrapplingHookProjectile = projectile;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(GrapplingHookProjectile);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            GrapplingHookProjectile.Deserialize(reader);
        }

        public int CompareTo(CreateGrapplingHookProjectileNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
