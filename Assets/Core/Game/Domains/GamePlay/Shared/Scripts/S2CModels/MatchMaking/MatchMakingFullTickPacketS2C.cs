using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.MatchMaking.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.PacketEvents.NetEvents;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking
{
    public class MatchMakingFullTickPacketS2C : INetSerializable
    {
        public int Tick;
        //public SimulationStateS2C PreviousSimulationState; // not sure if gonna need this
        public MatchMakingSimulationStateS2C CurrentSimulationState;
        public FixedUnorderedList<BulletSpawnNetEventS2C> BulletSpawnNetEvents; // todo: remove events related to bullet when bullet id destroyed
        public FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C> PlayerJoinAcceptNetEvents;
        public FixedUnorderedList<BulletDestroyedNetEventS2C> BulletDestroyedNetEvents;
        public FixedUnorderedList<PlayerSwitchTeamNetEventS2C> PlayerSwitchTeamNetEvents;
        public FixedUnorderedList<StartMatchCountdownNetEventS2C> StartMatchCountdownNetEvents;
        public FixedUnorderedList<StopMatchCountdownNetEventS2C> StopMatchCountdownNetEvents;
        public FixedUnorderedList<StartMatchEligibleChangedNetEventS2C> StartMatchEligibleChangedNetEvents;
        public FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C> PlayerLockOnTargetsChangedNetEvents;
        public FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C> PlayerLockedOnTargetHitNetEvents;

        public MatchMakingFullTickPacketS2C()
        {
        }

        public MatchMakingFullTickPacketS2C(MaxCap maxCap)
        {
            CurrentSimulationState = new MatchMakingSimulationStateS2C(maxCap.ConcurrentPlayers, maxCap.ConcurrentBullets);
            BulletSpawnNetEvents = new FixedUnorderedList<BulletSpawnNetEventS2C>(maxCap.BulletSpawnNetEvents);
            PlayerJoinAcceptNetEvents = new FixedClassUnorderedList<MatchMakingPlayerJoinAcceptPacketS2C>(maxCap.PlayerJoinAcceptNetEvents, () => new MatchMakingPlayerJoinAcceptPacketS2C(maxCap));
            BulletDestroyedNetEvents = new FixedUnorderedList<BulletDestroyedNetEventS2C>(maxCap.BulletDestroyedNetEvents);
            PlayerSwitchTeamNetEvents = new FixedUnorderedList<PlayerSwitchTeamNetEventS2C>(maxCap.PlayerSwitchTeamNetEvents);
            StartMatchCountdownNetEvents = new FixedUnorderedList<StartMatchCountdownNetEventS2C>(maxCap.StartMatchCountdownNetEvents);
            StopMatchCountdownNetEvents = new FixedUnorderedList<StopMatchCountdownNetEventS2C>(maxCap.StopMatchCountdownNetEvents);
            StartMatchEligibleChangedNetEvents = new FixedUnorderedList<StartMatchEligibleChangedNetEventS2C>(maxCap.StartMatchEligibleChangedNetEvents);
            PlayerLockOnTargetsChangedNetEvents = new FixedClassUnorderedList<PlayerLockOnTargetsChangedNetEventS2C>(maxCap.PlayerLockOnTargetsChangedNetEvents, () => new PlayerLockOnTargetsChangedNetEventS2C(maxCap.ConcurrentLockOnTargets));
            PlayerLockedOnTargetHitNetEvents = new FixedUnorderedList<PlayerLockedOnTargetHitNetEventS2C>(maxCap.PlayerLockOnTargetHitNetEvents);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            CurrentSimulationState.SerializeTransforms(writer);
            SerializedPlayerJoinedEvents(writer);
            SerializedBulletSpawnedEvents(writer);
            SerializedBulletDestroyedEvents(writer);
            SerializedPlayerSwitchTeamEvents(writer);
            SerializedStartMatchCountdownEvents(writer);
            SerializedStopMatchCountdownEvents(writer);
            SerializedStartMatchEligibleChangedEvents(writer);
            SerializedPlayerLockOnTargetsChangedEvents(writer);
            SerializedPlayerLockedOnTargetHitEvents(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            CurrentSimulationState.DeserializeTransforms(reader);
            DeserializedPlayerJoinedEvents(reader);
            DeserializedBulletSpawnedEvents(reader);
            DeserializedBulletDestroyedEvents(reader);
            DeserializedPlayerSwitchTeamEvents(reader);
            DeserializedStartMatchCountdownEvents(reader);
            DeserializedStopMatchCountdownEvents(reader);
            DeserializedStartMatchEligibleChangedEvents(reader);
            DeserializedPlayerLockOnTargetsChangedEvents(reader);
            DeserializedPlayerLockedOnTargetHitEvents(reader);
        }
        
        private void SerializedBulletDestroyedEvents(NetDataWriter writer)
        {
            writer.Put((byte) BulletDestroyedNetEvents.Count);
            foreach (var bulletDestroyedEvent in BulletDestroyedNetEvents.AsSpan())
            {
                bulletDestroyedEvent.Serialize(writer);
            }
        }

        private void DeserializedBulletDestroyedEvents(NetDataReader reader)
        {
            BulletDestroyedNetEvents.Clear();
            var bulletDestroyedEventsCount = reader.GetByte();
            for (var i = 0; i < bulletDestroyedEventsCount; i++)
            {
                ref var bulletDestroyedEvent = ref BulletDestroyedNetEvents.AddAndGet();
                bulletDestroyedEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerJoinedEvents(NetDataWriter writer)
        {
            writer.Put((byte) PlayerJoinAcceptNetEvents.Count);
            foreach (var playerJoinAcceptNetEvent in PlayerJoinAcceptNetEvents.AsSpan())
            {
                playerJoinAcceptNetEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerJoinedEvents(NetDataReader reader)
        {
            PlayerJoinAcceptNetEvents.Clear();
            var playerJoinedNetEventsCount = reader.GetByte();
            for (var i = 0; i < playerJoinedNetEventsCount; i++)
            {
                var playerJoinAcceptPacket = PlayerJoinAcceptNetEvents.AddAndGet();
                playerJoinAcceptPacket.Deserialize(reader);
            }
        }

        private void SerializedBulletSpawnedEvents(NetDataWriter writer)
        {
            var bulletSpawnedAmount = BulletSpawnNetEvents.Count;
            writer.Put((byte) bulletSpawnedAmount);
            foreach (var bulletSpawnEvent in BulletSpawnNetEvents.AsSpan())
            {
                bulletSpawnEvent.Serialize(writer);
            }
        }

        private void DeserializedBulletSpawnedEvents(NetDataReader reader)
        {
            BulletSpawnNetEvents.Clear();
            var bulletSpawnNetEventsCount = reader.GetByte();
            for (var i = 0; i < bulletSpawnNetEventsCount; i++)
            {
                ref var bulletSpawnEvent = ref BulletSpawnNetEvents.AddAndGet();
                bulletSpawnEvent.Deserialize(reader);
            }
        }

        private void SerializedPlayerSwitchTeamEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerSwitchTeamNetEvents.Count);
            foreach (var playerSwitchTeamNetEvent in PlayerSwitchTeamNetEvents.AsSpan())
            {
                playerSwitchTeamNetEvent.Serialize(writer);
            }
        }

        private void DeserializedPlayerSwitchTeamEvents(NetDataReader reader)
        {
            PlayerSwitchTeamNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var playerSwitchTeamNetEvent = ref PlayerSwitchTeamNetEvents.AddAndGet();
                playerSwitchTeamNetEvent.Deserialize(reader);
            }
        }

        private void SerializedStartMatchCountdownEvents(NetDataWriter writer)
        {
            writer.Put((byte)StartMatchCountdownNetEvents.Count);
            foreach (var evt in StartMatchCountdownNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void SerializedStopMatchCountdownEvents(NetDataWriter writer)
        {
            writer.Put((byte)StopMatchCountdownNetEvents.Count);
            foreach (var evt in StopMatchCountdownNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedStartMatchCountdownEvents(NetDataReader reader)
        {
            StartMatchCountdownNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref StartMatchCountdownNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void DeserializedStopMatchCountdownEvents(NetDataReader reader)
        {
            StopMatchCountdownNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref StopMatchCountdownNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedStartMatchEligibleChangedEvents(NetDataWriter writer)
        {
            writer.Put((byte)StartMatchEligibleChangedNetEvents.Count);
            foreach (var evt in StartMatchEligibleChangedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedStartMatchEligibleChangedEvents(NetDataReader reader)
        {
            StartMatchEligibleChangedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref StartMatchEligibleChangedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedPlayerLockOnTargetsChangedEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerLockOnTargetsChangedNetEvents.Count);
            foreach (var evt in PlayerLockOnTargetsChangedNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedPlayerLockOnTargetsChangedEvents(NetDataReader reader)
        {
            PlayerLockOnTargetsChangedNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                var evt = PlayerLockOnTargetsChangedNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }

        private void SerializedPlayerLockedOnTargetHitEvents(NetDataWriter writer)
        {
            writer.Put((byte)PlayerLockedOnTargetHitNetEvents.Count);
            foreach (var evt in PlayerLockedOnTargetHitNetEvents.AsSpan())
            {
                evt.Serialize(writer);
            }
        }

        private void DeserializedPlayerLockedOnTargetHitEvents(NetDataReader reader)
        {
            PlayerLockedOnTargetHitNetEvents.Clear();
            var count = reader.GetByte();
            for (var i = 0; i < count; i++)
            {
                ref var evt = ref PlayerLockedOnTargetHitNetEvents.AddAndGet();
                evt.Deserialize(reader);
            }
        }
    }
}