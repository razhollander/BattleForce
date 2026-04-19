
namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct ChickenEggModelS2C : LiteNetLib.Utils.INetSerializable
    {
        public ushort Id;
        public System.Numerics.Vector2 Position;
        public bool IsBroken;

        public void Serialize(LiteNetLib.Utils.NetDataWriter writer)
        {
            writer.Put(Id);
            writer.PutVector2(Position);
            writer.Put(IsBroken);
        }

        public void Deserialize(LiteNetLib.Utils.NetDataReader reader)
        {
            Id = reader.GetUShort();
            Position = reader.GetVector2();
            IsBroken = reader.GetBool();
        }
    }
}
