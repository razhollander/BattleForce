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
        public int ServerPlayerInputPacketsBuffer = 2;
        public int DefaultHostPort = 49153;
        public string IpAddress = "109.67.156.134";
        public bool OnlyLocal = false;
        public string ConntectionKey = "BattleForceGame";
        public int HeadlessQuitTimeoutSeconds = 30;
    }

    [Serializable]
    public class MaxCap
    {
        public int ConcurrentPlayers = 8;
        public int ConcurrentEnemyPlayers => ConcurrentPlayers - 1;
        public int ConcurrentLockOnTargets => ConcurrentEnemyPlayers + ConcurrentPowerUpBalls;
        public int ConcurrentBullets = 256;
        public int ConcurrentTalentCards = 128;
        public int ConcurrentEvironmentWalls = 256;
        public int ConcurrentEvironmentLavaWalls = 64;
        public int ConcurrentEvironmentStageBoundaries = 256;
        public int ConcurrentEvironmentTeleportPairs = 4;
        public int ConcurrentEvironmentSprings = 32;
        public int ConcurrentEvironmentSpikes = 32;
        public int ConcurrentChickenEggs = 128;
        public int PacketTypes = 256; // if one day this is changed to a bigger number, need to parse packet types as ushort instead of byte
        public int ConcurrentEnvironmentRotatingWheels = 16;
        public int ConcurrentFieldBarriers = 16;
        public EnvironmentRotatingWheel EnvironmentRotatingWheelCap;
        
        //physics Box2D
        public int ConcurrentTimeOfImpactContacts = 32;
        public int ConcurrentBodyCount = 512;
        public int ConcurrentContactCount = 256;
        public int ConcurrentFixuresCount = 512;
        public int ConcurrentPolygonCount = 256;
        public int ConcurrentCircleCount = 256;
        public int ConcurrentJointCount = 0;
        
        // packets receiived *all players combined*
        public int PlayersInputsPackets = 24000; // 5 seconds of packets
        public int JoinRequestPackets = 30; // 30 to stay on the same side, maybe should be ConcurrentPlayers
        public int JoinResponsePackets = 10;
        public int ConcurrentInputsProcessed = 200;
        
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
        public int ConcurrentPowerUpBalls = 32;
        public int PowerUpSpawnedNetEvents = 64;
        public int PowerUpObtainedNetEvents = 64;
        public int PlayerSwitchTeamNetEvents = 64;
        public int StartMatchCountdownNetEvents = 32;
        public int StopMatchCountdownNetEvents = 32;
        public int ExitMatchMakingNetEvents = 32;
        public int StartMatchNetEvents = 32;
        public int StageEndNetEvents = 32;
        public int PlayerDiedNetEvents = 32;
        public int StartMatchEligibleChangedNetEvents = 32;
        public int TalentSwitchNetEvents = 128;
        public int EnvironmentSpringPlayerCollisionNetEvents = 64;
        public int EnvironmentSpikePlayerCollisionNetEvents = 64;
        public int GainBoltsNetEvents = 128;
        public int PlayerToEnvironmentTeleportGateCollisionNetEvents = 64;
        public int PreparationPhaseEndedNetEvents = 32;
        public int CreateSwapFieldNetEvents = 128;
        public int DestroySwapFieldNetEvents = 128;
        public int KOProjectHitPlayerNetEvents = 128;
        public int CreateKOProjectileNetEvents = 128;
        public int DeactivateKOTalentNetEvents = 128;
        public int PerformDashPulseNetEvents = 128;
        public int UpdatePlayerTalentStocksNetEvents = 128;
        public int ActivateSentryGunTalentNetEvents = 128;
        public int DeactivateSentryGunTalentNetEvents = 128;
        public int PlayerGrapplingHookShotNetEvents = 128;
        public int PlayerGrapplingHookHitNetEvents = 128;
        public int PlayerGrapplingHookDeactivatedNetEvents = 128;
        public int UpdatePlayerTalentStocksNetEvent = 128;
        public int PlayerMaxShootCooldownChangedNetEvents = 128;
        public int PlayerSpinnedStartedNetEvents = 128;
        public int PlayerSpinnedEndedNetEvents = 128;
        public int CreateGrapplingHookProjectileNetEvents = 128;
        public int GrapplingHookHitWallNetEvents = 128;
        public int DeactivateGrapplingHookTalentNetEvents = 128;
        public int ActivateUmbrellaTalentNetEvents = 128;
        public int DeactivateUmbrellaTalentNetEvents = 128;
        public int CreateMagneticPullFieldNetEvents = 128;
        public int LayChickenEggNetEvents = 128;
        public int ChickenEggHitNetEvents = 128;
        public int ActivateYearsOfPainTalentNetEvents = 128;
        public int PlayerLockOnTargetsChangedNetEvents = 128;
        public int PlayerLockOnTargetHitNetEvents = 128;
        public int PlayerPowerUpChangedNetEvents = 64;
        public int ActivateSonicSlapNetEvents = 64;
        public int ActivateNukePowerUpNetEvents = 64;
        public int ActivateShufflePowerUpNetEvents = 64;
        public int ShuffleSwapPlayerPositionNetEvents = 128;
        public int ConcurrentGalacticForceFields = 16;
        public int PerformGalacticPullNetEvents = 64;
        public int DeactivateGalacticForceFieldNetEvents = 64;
        public int StartPowerUpGrantingPhaseNetEvents = 64;
        public int EndPowerUpGrantingPhaseNetEvents = 64;

        [Serializable]
        public class EnvironmentRotatingWheel
        {
            public int MaxWalls = 32;
            public int MaxLavaWalls = 16;
            public int MaxSprings = 8;
            public int MaxSpikes = 16;
            public int MaxTeleportGates = 8;
        }
    }
}