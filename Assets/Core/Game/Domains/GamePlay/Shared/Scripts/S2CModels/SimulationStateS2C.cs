using System.Numerics;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class SimulationStateS2C
    {
        public FixedClassUnorderedList<PlayerStateS2C> Players;
        public FixedUnorderedList<PlayerBulletS2C> Bullets;
        public FixedUnorderedList<EnvironmentWallStateS2C> Walls;

        public SimulationStateS2C(int maxPlayers, int maxBullets, int maxWalls, int maxPointsInWall, int maxTalents)
        {
            Players = new FixedClassUnorderedList<PlayerStateS2C>(maxPlayers, ()=>new PlayerStateS2C(maxTalents));
            Bullets = new FixedUnorderedList<PlayerBulletS2C>(maxBullets);
            Walls = new FixedUnorderedList<EnvironmentWallStateS2C>(maxWalls);
            
            for (int i = 0; i < Walls.Count; i++)
            {
                ref var wall = ref Walls.AddAndGet();
                wall.Points = new Vector2[maxPointsInWall];
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            var playerCount = Players.Count;
            writer.Put((byte)playerCount);
            foreach (var player in Players.AsSpan())
            {
                player.Serialize(writer);
            }
        
            var bulletsCount = Bullets.Count;
            writer.Put((byte)bulletsCount);
            foreach (var bullet in Bullets.AsSpan())
            {
                bullet.Serialize(writer);
            }
            
            var wallsCount = Walls.Count;
            writer.Put((byte)wallsCount);
            foreach (var wall in Walls.AsSpan())
            {
                wall.Serialize(writer);
            }
        }
        
        public void Deserialize(NetDataReader reader)
        {
            var playersCount = reader.GetByte();
            Players.Clear();
            for (var i = 0; i < playersCount; i++)
            {
                var player = Players.AddAndGet();
                player.Deserialize(reader);;
            }
          
            var bulletsCount = reader.GetByte();
            Bullets.Clear();
            for (var i = 0; i < bulletsCount; i++)
            {
                ref var bullet = ref Bullets.AddAndGet();
                bullet.Deserialize(reader);
            }
            
            var wallsCount = reader.GetByte();
            Walls.Clear();
            for (var i = 0; i < wallsCount; i++)
            {
                ref var wall = ref Walls.AddAndGet();
                wall.Deserialize(reader);
            }
        }

        public PlayerStateS2C GetPlayerById(ushort playerId)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Id == playerId)
                {
                    return Players.GetByIndex(i);
                } 
            }

            throw new System.Exception($"No player for id {playerId}!");
        }

        public PlayerStateS2C GetPlayerByIndex(int index)
        {
            return Players.GetByIndex(index);
        }

        public void RemoveBulletById(ushort bulletId)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (Bullets[i].Id == bulletId)
                {
                    Bullets.RemoveAt(i);
                    return;
                } 
            }
            
            throw new System.Exception($"No bullet for id {bulletId}!");
        }

        public ref PlayerBulletS2C GetBulletById(ushort bulletId)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (Bullets[i].Id == bulletId)
                {
                    return ref Bullets.GetByIndex(i);
                } 
            }
            
            throw new System.Exception("No bullet for id {playerId}!");
        }

        public ref PlayerBulletS2C GetBulletByIndex(int index)
        {
            return ref Bullets.GetByIndex(index);
        }

        public void AddWall(ushort wallId, Vector2[] wallPoints)
        {
            ref var wallState = ref Walls.AddAndGet();
            wallState.Id = wallId;
            wallState.Points = wallPoints;
        }
        
        public void SerializeTransforms(NetDataWriter writer)
        {
            var playerCount = Players.Count;
            writer.Put((byte) playerCount);
            foreach (var player in Players.AsSpan())
            {
                player.SerializeDeltas(writer);
            }

            var bulletsCount = Bullets.Count;
            writer.Put((byte) bulletsCount);
            foreach (var bullet in Bullets.AsSpan())
            {
                bullet.SerializeTransforms(writer);
            }
        }

        public void DeserializeTransforms(NetDataReader reader)
        {
            var playersCount = reader.GetByte();
            Players.Clear();
            for (var i = 0; i < playersCount; i++)
            {
                var player = Players.AddAndGet();
                player.DeserializeDeltas(reader);
            }

            var bulletsCount = reader.GetByte();
            Bullets.Clear();
            for (int i = 0; i < bulletsCount; i++)
            {
                ref var bullet = ref Bullets.AddAndGet();
                bullet.DeserializeTransforms(reader);
            }
        }
    }
}