using LiteNetLib.Utils;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct PlayerLockOnHeartTargetsChangedNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public FixedUnorderedList<ushort> PlayersHeartsLockOnTargets;

        public PlayerLockOnHeartTargetsChangedNetEventS2C(int maxTargets)
        {
            OccuredOnTick = 0;
            PlayerId = 0;
            PlayersHeartsLockOnTargets = new FixedUnorderedList<ushort>(maxTargets);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PlayerId);
            writer.Put((byte)PlayersHeartsLockOnTargets.Count);
            foreach (var target in PlayersHeartsLockOnTargets.AsSpan())
            {
                writer.Put(target);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetUShort();
            PlayersHeartsLockOnTargets.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var target = ref PlayersHeartsLockOnTargets.AddAndGet();
                target = reader.GetUShort();
            }
        }
    }
}
