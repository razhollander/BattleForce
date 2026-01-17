using System;
using UnityEngine;

namespace Core.Scripts.Network
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "BF/Network/Network Config")]
    public class NetworkConfig : ScriptableObject
    {
        public MaxCap MaxCap;
        public int TicksPerSeconds = 60;
        public float DeltaTime = 1/60f;
        public int PhysicsVelocityIterations = 8;
        public int PositionIterations = 8;
        public int ServerTicksBuffer = 2;
        public int HostPort = 49153;
        public string IpAddress = "109.67.156.134";
        public string ConntectionKey = "BattleForceGame";
    }

    [Serializable]
    public class MaxCap
    {
        public int ConcurrentPlayers = 8;
        public int ConcurrentBullets = 256;
        public int ConcurrentTalentCards = 128;
        public int ConcurrentEvironmentWalls = 64;
        public int PointsInEvironmentWall = 8;
        public int PacketTypes = 256; // if one day this is changed to a bigger number, need to parse packet types as ushort instead of byte
        
        //physics Box2D
        public int ConcurrentTimeOfImpactContacts = 32;
        public int ConcurrentBodyCount = 512;
        public int ConcurrentContactCount = 256;
        public int ConcurrentJointCount = 0;
        
        // packets receiived *all players combined*
        public int PlayersInputsPackets = 24000; // 5 seconds of packets
        public int JoinRequestPackets = 30; // 30 to stay on the same side, maybe should be ConcurrentPlayers
        public int ConcurrentInputsProcessed = 100;
        
        // events received *per player*
        public int FullTickPacketsNetEvents = 1000;
        public int PlayerJoinAcceptNetEvents = 32;
        public int PlayerTakeDamageNetEvents = 128;
        public int BulletSpawnNetEvents = 512;
        public int BulletDestroyedNetEvents = 512;
        public int PlayerSwapNetEvents = 64;
        public int TalentCardObtainedNetEvent = 64;
        public int MaxCollisionsPerFrame = 256;
        public int TalentCardHitNetEvents = 128;
    }
}