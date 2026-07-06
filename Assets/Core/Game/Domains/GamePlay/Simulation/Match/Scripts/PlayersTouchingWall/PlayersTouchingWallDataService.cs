using System.Collections.Generic;
using System.Numerics;
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

        public void OnPlayerBeginTouchWall(ushort playerId, ushort wallId, Vector2 wallNormalWhenTouchBegin, float wallRotationDegreesWhenTouchBegin, int tick)
        {
            if (!_playersTouchingWall.ContainsKey(playerId))
            {
                _playersTouchingWall.Add(playerId, _playerDataPool.Get());
            }

            _playersTouchingWall[playerId].OnBeginTouchWall(wallId, wallNormalWhenTouchBegin, wallRotationDegreesWhenTouchBegin, tick);
        }

        public void OnPlayerEndTouchWall(ushort playerId, ushort wallId)
        {
            if (!_playersTouchingWall.ContainsKey(playerId))
            {
                LogService.LogError($"Player {playerId} stopped touching wall {wallId} but does not exist in touching wall tracker");
                return;
            }

            var playerData = _playersTouchingWall[playerId];
            playerData.OnEndTouchWall(wallId);

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

        private class PlayerTouchingWallsData
        {
            private readonly CapacityDict<ushort, WallTouchData> _walls;
            private readonly ConcurrentPool<WallTouchData> _wallDataPool;

            public int TouchingWallsCount => _walls.Count;

            public PlayerTouchingWallsData(int maxWalls)
            {
                _walls = new CapacityDict<ushort, WallTouchData>(maxWalls);
                _wallDataPool = new ConcurrentPool<WallTouchData>(() => new WallTouchData(), maxWalls);
            }

            public void OnBeginTouchWall(ushort wallId, Vector2 wallNormalWhenTouchBegin, float wallRotationDegreesWhenTouchBegin, int tick)
            {
                if (!_walls.ContainsKey(wallId))
                {
                    var data = _wallDataPool.Get();
                    data.WallId = wallId;
                    data.WallLocalNormal = wallNormalWhenTouchBegin.Rotate(-wallRotationDegreesWhenTouchBegin);
                    data.BeginTick = tick;
                    data.ContactCount = 0;
                    _walls.Add(wallId, data);
                }

                _walls[wallId].ContactCount++;
            }

            public void OnEndTouchWall(ushort wallId)
            {
                if (!_walls.ContainsKey(wallId))
                {
                    LogService.LogError($"Wall {wallId} contact ended but was not tracked for this player");
                    return;
                }

                var data = _walls[wallId];
                var contactCount = --data.ContactCount;
                if (contactCount <= 0)
                {
                    data.Reset();
                    _wallDataPool.Return(data);
                    _walls.Remove(wallId);
                }
            }

            public void GetPlayersStickToWalls(ushort playerId, int currentTick, int minTicksTouching, List<PlayerStickToWallData> output)
            {
                foreach (var wallId in _walls.Keys)
                {
                    var data = _walls[wallId];
                    var ticksTouching = currentTick - data.BeginTick;
                    if (ticksTouching >= minTicksTouching)
                    {
                        output.Add(new PlayerStickToWallData(playerId, wallId, data.WallLocalNormal));
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
            public ushort WallId;
            public Vector2 WallLocalNormal;
            public int BeginTick;
            public int ContactCount;

            public void Reset()
            {
                WallId = 0;
                WallLocalNormal = Vector2.Zero;
                BeginTick = 0;
                ContactCount = 0;
            }
        }
    }
}
