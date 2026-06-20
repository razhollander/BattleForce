using Core.Game.Domains.GamePlay.Shared.S2CModels;
using LiteNetLib.Utils;
namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking
{
    public class MatchMakingPlayerSpaceshipStateS2C : INetSerializable
    {
        public PlayerTransformStateS2C Transform;
        public PlayerShootStateS2C Shoot;
        public bool IsLockingOnWall;
        public bool IsWallShootable;

        public MatchMakingPlayerSpaceshipStateS2C()
        {
        }

        public void Serialize(NetDataWriter writer)
        {
            Transform.Serialize(writer);
            Shoot.Serialize(writer);
            writer.Put(IsLockingOnWall);
            writer.Put(IsWallShootable);
        }

        public void Deserialize(NetDataReader reader)
        {
            Transform.Deserialize(reader);
            Shoot.Deserialize(reader);
            IsLockingOnWall = reader.GetBool();
            IsWallShootable = reader.GetBool();
        }

        public void SerializeDeltas(NetDataWriter writer)
        {
            Transform.SerializeDeltas(writer);
            Shoot.SerializeDeltas(writer);
            writer.Put(IsLockingOnWall);
            writer.Put(IsWallShootable);
        }

        public void DeserializeDeltas(NetDataReader reader)
        {
            Transform.DeserializeDeltas(reader);
            Shoot.DeserializeDeltas(reader);
            IsLockingOnWall = reader.GetBool();
            IsWallShootable = reader.GetBool();
        }

        public MatchMakingPlayerSpaceshipStateS2C GetClone()
        {
            return new MatchMakingPlayerSpaceshipStateS2C()
            {
                Shoot = this.Shoot,
                Transform = this.Transform,
                IsLockingOnWall = this.IsLockingOnWall,
                IsWallShootable = this.IsWallShootable
            };
        }
    }
}