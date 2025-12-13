using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.MatchModel
{
    public class MatchNetEventsDataService : IMatchNetEventsDataService
    {
        public Dictionary<ushort, List<BulletSpawnNetEventS2C>> BulletSpawnNetEventsPerPlayer { get; private set; } = new (); // todo: remove events related to bullet when bullet is destroyed
        public Dictionary<ushort, List<PlayerJoinAcceptPacketS2C>> JoinAcceptNetEventsPerPlayer { get; private set; } = new (); // todo: remove events related to player when player is destroyed

        public void StartSavingPlayerEvents(ushort playerId)
        {
            if (!BulletSpawnNetEventsPerPlayer.TryAdd(playerId, new List<BulletSpawnNetEventS2C>()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
            
            if (!JoinAcceptNetEventsPerPlayer.TryAdd(playerId, new List<PlayerJoinAcceptPacketS2C>()))
            {
                LogService.LogError($"Player already exists! {playerId}");
            }
        }
        
        public void StopSavingPlayerEvents(ushort playerId)
        {
            BulletSpawnNetEventsPerPlayer.Remove(playerId);
            JoinAcceptNetEventsPerPlayer.Remove(playerId);
        }
        
        public void AddBulletSpawnNetEvent(int onTick, int bulletId, ushort belongToPlayerId, Vector2 position)
        {
            foreach (var kvp in BulletSpawnNetEventsPerPlayer)
            {
                kvp.Value.Add(new BulletSpawnNetEventS2C(onTick, bulletId, belongToPlayerId, position));
            }
        }

        public void RemoveAllEventsOlderThanTick(ushort playerId, int tick)
        {
            if (BulletSpawnNetEventsPerPlayer.TryGetValue(playerId, out var bulletSpawnNetEvents))
            {
                bulletSpawnNetEvents.RemoveAll(x => x.OccuredOnTick < tick);
            }
            if (JoinAcceptNetEventsPerPlayer.TryGetValue(playerId, out var joinAcceptNetEvents))
            {
                joinAcceptNetEvents.RemoveAll(x => x.OccuredOnTick < tick);
            }
        }

        public void AddPlayerJoinAcceptedEvent(int onTick, int netPeerId, string playerName, PlayerSpaceshipStateS2C playerSpaceshipState, ushort playerId)
        {
            foreach (var kvp in JoinAcceptNetEventsPerPlayer)
            {
                kvp.Value.Add(new PlayerJoinAcceptPacketS2C
                {
                    OccuredOnTick = onTick,
                    NetPeerId = netPeerId,
                    PlayerId = playerId,
                    PlayerName = playerName,
                    SpaceshipState = playerSpaceshipState
                });
            }
        }
    }
}