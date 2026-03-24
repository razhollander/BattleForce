using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents
{
    public struct CreateKOProjectileNetEventS2C : INetSerializable, IComparable<CreateKOProjectileNetEventS2C>
    {
        public int OccuredOnTick;
        public TalentKOProjectileS2C KoProjectile;

        public CreateKOProjectileNetEventS2C(int occuredOnTick, TalentKOProjectileS2C koProjectile)
        {
            OccuredOnTick = occuredOnTick;
            KoProjectile = koProjectile;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(KoProjectile);
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            KoProjectile.Deserialize(reader);
        }

        public int CompareTo(CreateKOProjectileNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
