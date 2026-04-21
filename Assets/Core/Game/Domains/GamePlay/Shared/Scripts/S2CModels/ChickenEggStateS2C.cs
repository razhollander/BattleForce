using System.Numerics;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct ChickenEggStateS2C : INetSerializable
    {
        public ushort Id;
        public ushort CasterPlayerId;
        public Vector2 Position;

        public void Serialize(NetDataWriter writer)
        {
          
        }

        public void Deserialize(NetDataReader reader)
        {
            
        }
    }
}
