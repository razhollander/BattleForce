using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking
{
    public class MatchMakingSimulationStateS2C
    {
        public FixedClassUnorderedList<MatchMakingPlayerStateS2C> Players;
        public FixedUnorderedList<PlayerBulletS2C> Bullets;
        public MatchMakingStartMatchWallS2C StartMatchWall;

        public MatchMakingSimulationStateS2C(int maxPlayers, int maxBullets)
        {
            Players = new FixedClassUnorderedList<MatchMakingPlayerStateS2C>(maxPlayers, ()=>new MatchMakingPlayerStateS2C());
            Bullets = new FixedUnorderedList<PlayerBulletS2C>(maxBullets);
            StartMatchWall = new MatchMakingStartMatchWallS2C();
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
            
            StartMatchWall.Serialize(writer);
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
            
            StartMatchWall.Deserialize(reader);
        }

        public MatchMakingPlayerStateS2C GetPlayerById(ushort playerId)
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
        
        public bool TryGetPlayerByName(string playerName, out MatchMakingPlayerStateS2C playerState)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name == playerName)
                {
                    playerState = Players.GetByIndex(i);
                    return true;
                } 
            }

            playerState = default;
            return false;
        }

        public MatchMakingPlayerStateS2C GetPlayerByIndex(int index)
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
        
        public bool TryGetBulletById(ushort bulletId, out PlayerBulletS2C bulletState)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                bulletState = Bullets[i];
                if (bulletState.Id == bulletId)
                {
                    return true;
                } 
            }

            bulletState = default;
            return false;
        }
        
        public bool TryGetBulletIndexById(ushort bulletId, out int  index)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                if (Bullets[i].Id == bulletId)
                {
                    index = i;
                    return true;
                } 
            }

            index = -1;
            return false;
        }
        
        public ref PlayerBulletS2C GetBulletByIndex(int index)
        {
            return ref Bullets.GetByIndex(index);
        }
        
        public void SerializeTransforms(NetDataWriter writer)
        {
            var playerCount = Players.Count;
            writer.Put((byte) playerCount);
            foreach (var player in Players.AsSpan())
            {
                player.SerializeDeltas(writer);
            }

            // var bulletsCount = Bullets.Count;
            // writer.Put((byte) bulletsCount);
            // foreach (var bullet in Bullets.AsSpan())
            // {
            //     bullet.SerializeTransforms(writer);
            // }
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

            // var bulletsCount = reader.GetByte();
            // Bullets.Clear();
            // for (int i = 0; i < bulletsCount; i++)
            // {
            //     ref var bullet = ref Bullets.AddAndGet();
            //     bullet.DeserializeTransforms(reader);
            // }
        }
    }
}