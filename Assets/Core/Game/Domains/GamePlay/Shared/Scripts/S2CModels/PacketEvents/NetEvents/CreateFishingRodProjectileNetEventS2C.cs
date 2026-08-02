using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateFishingRodProjectileNetEventS2C : INetSerializable, IComparable<CreateFishingRodProjectileNetEventS2C>
    {
        public int OccuredOnTick;
        public TalentFishingRodProjectileStateS2C FishingRodProjectile;

        public CreateFishingRodProjectileNetEventS2C(int occuredOnTick, TalentFishingRodProjectileStateS2C projectile)
        {
            OccuredOnTick = occuredOnTick;
            FishingRodProjectile = projectile;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(FishingRodProjectile);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            FishingRodProjectile.Deserialize(reader);
        }

        public int CompareTo(CreateFishingRodProjectileNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
