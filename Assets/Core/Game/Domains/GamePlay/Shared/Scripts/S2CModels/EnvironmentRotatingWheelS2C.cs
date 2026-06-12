using System;
using System.Numerics;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    public class EnvironmentRotatingWheelS2C : IEquatable<ushort>
    {
        public ushort Id;
        public Vector2 CenterPosition;
        public float RotationSpeed;
        public FixedUnorderedList<ushort> WallIds;
        public FixedUnorderedList<ushort> LavaWallIds;
        public FixedUnorderedList<ushort> SpringIds;
        public FixedUnorderedList<ushort> SpikeIds;
        public FixedUnorderedList<ushort> TeleportGatePairIds;

        public EnvironmentRotatingWheelS2C(MaxCap.EnvironmentRotatingWheel maxCap)
        {
            WallIds = new FixedUnorderedList<ushort>(maxCap.MaxWalls);
            LavaWallIds = new FixedUnorderedList<ushort>(maxCap.MaxLavaWalls);
            SpringIds = new FixedUnorderedList<ushort>(maxCap.MaxSprings);
            SpikeIds = new FixedUnorderedList<ushort>(maxCap.MaxSpikes);
            TeleportGatePairIds = new FixedUnorderedList<ushort>(maxCap.MaxTeleportGatePairs);
        }

        public void ClearData()
        {
            WallIds.Clear();
            LavaWallIds.Clear();
            SpringIds.Clear();
            SpikeIds.Clear();
            TeleportGatePairIds.Clear();
        }
        
        public bool Equals(ushort otherId)
        {
            return Id == otherId;
        }

        public void AddWall(ushort wallId)
        {
            ref var wall = ref WallIds.AddAndGet();
            wall = wallId;
        }

        public void AddLavaWall(ushort lavaWallId)
        {
            ref var lavaWall = ref LavaWallIds.AddAndGet();
            lavaWall = lavaWallId;
        }
        
        public void AddSpring(ushort springId)
        {
            ref var spring = ref SpringIds.AddAndGet();
            spring = springId;
        }
        
        public void AddSpike(ushort spikeId)
        {
            ref var spike = ref SpikeIds.AddAndGet();
            spike = spikeId;
        }

        public void AddTeleportGatePair(ushort teleportGatePairId)
        {
            ref var gatePair = ref TeleportGatePairIds.AddAndGet();
            gatePair = teleportGatePairId;
        }
    }
}
