using LiteNetLib.Utils;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class PlayerLockOnHeartTargetsChangedNetEventS2C : INetSerializable
    {
        public ushort PlayerId;
        public FixedUnorderedList<ushort> LockedOnHeartIds;

        public PlayerLockOnHeartTargetsChangedNetEventS2C(int maxHeartsIdsOnTarget)
        {
            PlayerId = 0;
            LockedOnHeartIds = new FixedUnorderedList<ushort>(maxHeartsIdsOnTarget);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put((byte)LockedOnHeartIds.Count);
            for (int i = 0; i < LockedOnHeartIds.Count; i++)
            {
                writer.Put(LockedOnHeartIds[i]);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetUShort();
            LockedOnHeartIds.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                LockedOnHeartIds.Add(reader.GetUShort());
            }
        }
    }
}
