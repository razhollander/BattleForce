using System.Linq;
using System.Numerics;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public struct SimulationStateS2C
    {
        public ushort PlayersCount;
        public PlayerStateS2C[] Players;
        public StructPool<PlayerBulletS2C> Bullets;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PlayersCount);
            for (int i = 0; i < PlayersCount; i++)
            {
                Players[i].Serialize(writer);
            }
        
            var bulletsCount = Bullets.UsedCount;
            writer.Put((byte)bulletsCount);
            if (bulletsCount > 0)
            {
                foreach (var bulletIndex in Bullets.UsedIndices())
                {
                    Bullets[bulletIndex].Serialize(writer);
                }
            }
        }
        
        public void Deserialize(NetDataReader reader)
        {
            PlayersCount = reader.GetByte();
            Players = new PlayerStateS2C[PlayersCount];
            for (int i = 0; i < PlayersCount; i++)
                Players[i].Deserialize(reader);
            var bulletsCount = (int)reader.GetByte();
            Bullets = new StructPool<PlayerBulletS2C>(bulletsCount);
            if (bulletsCount > 0)
            {
                for (int i = 0; i < bulletsCount; i++)
                {
                    Bullets.Rent(out int index);
                    Bullets[index].Deserialize(reader);
                }
            }
        }

        public PlayerStateS2C GetPlayer(int playerId)
        {
            return Players.First(x => x.Id == playerId);
        }
        
        public PlayerBulletS2C GetBullet(int bulletId)
        {
            foreach (var index in Bullets.UsedIndices())
            {
                var playerBullet = Bullets[index];
                if (playerBullet.Id == bulletId)
                {
                    return playerBullet;
                }
            }

            LogService.LogError($"No bullet for id {bulletId}!");
            return default;
        }

        public void SerializeTransforms(NetDataWriter writer)
        {
            writer.Put((byte)PlayersCount);
            for (var i = 0; i < PlayersCount; i++)
            {
                Players[i].SerializeDeltas(writer);
            }

            var bulletsCount = Bullets.UsedCount;
            writer.Put((byte)bulletsCount);
            if (bulletsCount > 0)
            {
                foreach (var bulletIndex in Bullets.UsedIndices())
                {
                    Bullets[bulletIndex].SerializeTransforms(writer);
                }
            }
        }

        public void DeserializeTransforms(NetDataReader reader)
        {
            PlayersCount = reader.GetByte();
            Players = new PlayerStateS2C[PlayersCount];
            for (var i = 0; i < PlayersCount; i++)
            {
                Players[i].DeserializeDeltas(reader);
            }

            var bulletsCount = (int)reader.GetByte();
            Bullets = new StructPool<PlayerBulletS2C>(bulletsCount);
            if (bulletsCount > 0)
            {
                for (int i = 0; i < bulletsCount; i++)
                {
                    Bullets.Rent(out int index);
                    Bullets[index].DeserializeTransforms(reader);
                }
            }
        }
    }
}