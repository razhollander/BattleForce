using System;
using System.Collections.Generic;
using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Scripts.Extensions;
using Core.Scripts.Network;
using Core.Scripts.Utils;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingWall
{
    public class PlayersTouchingWallDataService : IPlayersTouchingWallDataService
    {
        private const int MAX_WALLS_PER_PLAYER = 16;

        private readonly CapacityDict<ushort, PlayerTouchingWallsData> _playersTouchingWall;
        private readonly ConcurrentPool<PlayerTouchingWallsData> _playerDataPool;
        private readonly List<PlayerStickToWallData> _cachedPlayersStickToWall;

        public PlayersTouchingWallDataService(NetworkConfig networkConfig)
        {
            var maxPlayers = networkConfig.MaxCap.ConcurrentPlayers;
            _playersTouchingWall = new CapacityDict<ushort, PlayerTouchingWallsData>(maxPlayers);
            _playerDataPool = new ConcurrentPool<PlayerTouchingWallsData>(() => new PlayerTouchingWallsData(MAX_WALLS_PER_PLAYER), maxPlayers);
            _cachedPlayersStickToWall = new List<PlayerStickToWallData>(maxPlayers * MAX_WALLS_PER_PLAYER);
        }

        public void OnPlayerBeginTouchWall(ushort playerId, PhysicsBodyType wallBodyType, ushort wallId, Vector2 wallNormalWhenTouchBegin, float wallRotationDegreesWhenTouchBegin, int tick)
        {
            if (!_playersTouchingWall.ContainsKey(playerId))
            {
                _playersTouchingWall.Add(playerId, _playerDataPool.Get());
            }

            _playersTouchingWall[playerId].OnBeginTouchWall(new WallTouchKey(wallBodyType, wallId), wallNormalWhenTouchBegin, wallRotationDegreesWhenTouchBegin, tick);
        }

        public void OnPlayerEndTouchWall(ushort playerId, PhysicsBodyType wallBodyType, ushort wallId)
        {
            if (!_playersTouchingWall.ContainsKey(playerId))
            {
                LogService.LogError($"Player {playerId} stopped touching wall {wallId} but does not exist in touching wall tracker");
                return;
            }

            var playerData = _playersTouchingWall[playerId];
            playerData.OnEndTouchWall(new WallTouchKey(wallBodyType, wallId));

            if (playerData.TouchingWallsCount == 0)
            {
                playerData.Reset();
                _playerDataPool.Return(playerData);
                _playersTouchingWall.Remove(playerId);
            }
        }

        public List<PlayerStickToWallData> GetPlayersStickToWall(int currentTick, int minTicksTouching)
        {
            _cachedPlayersStickToWall.Clear();

            foreach (var playerId in _playersTouchingWall.Keys)
            {
                _playersTouchingWall[playerId].GetPlayersStickToWalls(playerId, currentTick, minTicksTouching, _cachedPlayersStickToWall);
            }

            return _cachedPlayersStickToWall;
        }

        public void ClearAllData()
        {
            foreach (var kvp in _playersTouchingWall)
            {
                kvp.Value.Reset();
                _playerDataPool.Return(kvp.Value);
            }

            _playersTouchingWall.Clear();
        }

        // Wall ids and FrigidBlock ids are handed out by different systems and overlap, so a touch is identified by both.
        private readonly struct WallTouchKey : IEquatable<WallTouchKey>
        {
            public readonly PhysicsBodyType WallBodyType;
            public readonly ushort WallId;

            public WallTouchKey(PhysicsBodyType wallBodyType, ushort wallId)
            {
                WallBodyType = wallBodyType;
                WallId = wallId;
            }

            public bool Equals(WallTouchKey other)
            {
                return WallBodyType == other.WallBodyType && WallId == other.WallId;
            }

            public override bool Equals(object obj)
            {
                return obj is WallTouchKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return ((int) WallBodyType << 16) | WallId;
            }
        }

        private class PlayerTouchingWallsData
        {
            private readonly CapacityDict<WallTouchKey, WallTouchData> _walls;
            private readonly ConcurrentPool<WallTouchData> _wallDataPool;

            public int TouchingWallsCount => _walls.Count;

            public PlayerTouchingWallsData(int maxWalls)
            {
                _walls = new CapacityDict<WallTouchKey, WallTouchData>(maxWalls);
                _wallDataPool = new ConcurrentPool<WallTouchData>(() => new WallTouchData(), maxWalls);
            }

            public void OnBeginTouchWall(WallTouchKey wallKey, Vector2 wallNormalWhenTouchBegin, float wallRotationDegreesWhenTouchBegin, int tick)
            {
                if (!_walls.ContainsKey(wallKey))
                {
                    var data = _wallDataPool.Get();
                    data.WallLocalNormal = wallNormalWhenTouchBegin.Rotate(-wallRotationDegreesWhenTouchBegin);
                    data.BeginTick = tick;
                    data.ContactCount = 0;
                    _walls.Add(wallKey, data);
                }

                _walls[wallKey].ContactCount++;
            }

            public void OnEndTouchWall(WallTouchKey wallKey)
            {
                if (!_walls.ContainsKey(wallKey))
                {
                    LogService.LogError($"Wall {wallKey.WallId} of type {wallKey.WallBodyType} contact ended but was not tracked for this player");
                    return;
                }

                var data = _walls[wallKey];
                var contactCount = --data.ContactCount;
                if (contactCount <= 0)
                {
                    data.Reset();
                    _wallDataPool.Return(data);
                    _walls.Remove(wallKey);
                }
            }

            public void GetPlayersStickToWalls(ushort playerId, int currentTick, int minTicksTouching, List<PlayerStickToWallData> output)
            {
                foreach (var kvp in _walls)
                {
                    var data = kvp.Value;
                    var ticksTouching = currentTick - data.BeginTick;
                    if (ticksTouching >= minTicksTouching)
                    {
                        output.Add(new PlayerStickToWallData(playerId, kvp.Key.WallBodyType, kvp.Key.WallId, data.WallLocalNormal));
                    }
                }
            }

            public void Reset()
            {
                foreach (var kvp in _walls)
                {
                    kvp.Value.Reset();
                    _wallDataPool.Return(kvp.Value);
                }

                _walls.Clear();
            }
        }

        private class WallTouchData
        {
            public Vector2 WallLocalNormal;
            public int BeginTick;
            public int ContactCount;

            public void Reset()
            {
                WallLocalNormal = Vector2.Zero;
                BeginTick = 0;
                ContactCount = 0;
            }
        }
    }
}
