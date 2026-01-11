using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels
{
    public class SimulationStateS2C
    {
        public FixedClassUnorderedList<PlayerStateS2C> Players;
        public FixedUnorderedList<PlayerBulletS2C> Bullets;
        public FixedUnorderedList<TalentCardS2C> TalentCards;
        public int EnvironmentWallsIndex;

        public SimulationStateS2C(int maxPlayers, int maxBullets, int maxTalentsPerPlayer, int maxTalentCards)
        {
            Players = new FixedClassUnorderedList<PlayerStateS2C>(maxPlayers, ()=>new PlayerStateS2C(maxTalentsPerPlayer));
            Bullets = new FixedUnorderedList<PlayerBulletS2C>(maxBullets);
            TalentCards = new FixedUnorderedList<TalentCardS2C>(maxTalentCards);
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
            
            var talentCardsCount = TalentCards.Count;
            writer.Put((byte)talentCardsCount);
            foreach (var talentCard in TalentCards.AsSpan())
            {
                talentCard.Serialize(writer);
            }

            writer.Put((byte)EnvironmentWallsIndex);
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

            var talentCardsCount = reader.GetByte();
            TalentCards.Clear();
            for (var i = 0; i < talentCardsCount; i++)
            {
                ref var talentCard = ref TalentCards.AddAndGet();
                talentCard.Deserialize(reader);
            }

            EnvironmentWallsIndex = reader.GetByte();
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