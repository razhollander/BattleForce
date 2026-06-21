using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;
namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking
{
    public class MatchMakingPlayerSpaceshipStateS2C : INetSerializable
    {
        private const int MAX_LOCK_ON_TARGETS = 1;

        public PlayerTransformStateS2C Transform;
        public PlayerShootStateS2C Shoot;
        public readonly FixedUnorderedList<ObjectLockedOnTargetS2C> ObjectsLockedOnTarget;

        public bool IsLockingOnWall => ObjectsLockedOnTarget.Count > 0;
        public bool IsLockingOnWallShootable => ObjectsLockedOnTarget.Count > 0 && ObjectsLockedOnTarget[0].IsLockOnTargetShootable;

        public MatchMakingPlayerSpaceshipStateS2C()
        {
            ObjectsLockedOnTarget = new FixedUnorderedList<ObjectLockedOnTargetS2C>(MAX_LOCK_ON_TARGETS);
        }

        public void Serialize(NetDataWriter writer)
        {
            Transform.Serialize(writer);
            Shoot.Serialize(writer);

            writer.Put((byte)ObjectsLockedOnTarget.Count);
            for (int i = 0; i < ObjectsLockedOnTarget.Count; i++)
            {
                var targetedEnemy = ObjectsLockedOnTarget[i];
                writer.Put((byte)targetedEnemy.PlayerTargetId);
                writer.Put(targetedEnemy.IsLockOnTargetShootable);
                writer.Put((byte)targetedEnemy.TargetType);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Transform.Deserialize(reader);
            Shoot.Deserialize(reader);

            var targetedEnemyIdsAmount = reader.GetByte();
            ObjectsLockedOnTarget.Clear();
            for (int i = 0; i < targetedEnemyIdsAmount; i++)
            {
                ref var targetedEnemy = ref ObjectsLockedOnTarget.AddAndGet();
                targetedEnemy.PlayerTargetId = reader.GetByte();
                targetedEnemy.IsLockOnTargetShootable = reader.GetBool();
                targetedEnemy.TargetType = (LockOnTargetType)reader.GetByte();
            }
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            Transform.SerializeDeltas(writer);
            Shoot.SerializeDeltas(writer);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Transform.DeserializeDeltas(reader);
            Shoot.DeserializeDeltas(reader);
        }

        public MatchMakingPlayerSpaceshipStateS2C GetClone()
        {
            var clone = new MatchMakingPlayerSpaceshipStateS2C()
            {
                Shoot = this.Shoot,
                Transform = this.Transform,
            };

            clone.ObjectsLockedOnTarget.Clear();
            for (int i = 0; i < ObjectsLockedOnTarget.Count; i++)
            {
                ref var targetedEnemyId = ref clone.ObjectsLockedOnTarget.AddAndGet();
                targetedEnemyId = this.ObjectsLockedOnTarget[i];
            }

            return clone;
        }
    }
}
