using LiteNetLib.Utils;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class PlayerLockOnHeartTargetsChangedNetEventS2C : INetSerializable
    {
        public int OccuredOnTick;
        public ushort PlayerId;
        public FixedUnorderedList<ushort> PlayerIdsLockedOnTarget;

        public PlayerLockOnHeartTargetsChangedNetEventS2C(int maxHeartsIdsOnTarget)
        {
            PlayerId = 0;
            PlayerIdsLockedOnTarget = new FixedUnorderedList<ushort>(maxHeartsIdsOnTarget);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(PlayerId);
            writer.Put((byte)PlayerIdsLockedOnTarget.Count);
            for (int i = 0; i < PlayerIdsLockedOnTarget.Count; i++)
            {
                writer.Put(PlayerIdsLockedOnTarget[i]);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            PlayerId = reader.GetUShort();
            PlayerIdsLockedOnTarget.Clear();
            var count = reader.GetByte();
            for (int i = 0; i < count; i++)
            {
                ref var lockOnHeartId = ref PlayerIdsLockedOnTarget.AddAndGet();
                lockOnHeartId = reader.GetUShort();
            }
        }
    }
}
