import re

with open('./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs', 'r') as f:
    content = f.read()

# Replace props
content = content.replace(
    "public CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerPlayer { get; }",
    "public CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerPlayer { get; }\n        public CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>> CreateKOProjectileNetEventsPerPlayer { get; }\n        public CapacityDict<ushort, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> KOProjectHitPlayerNetEventsPerPlayer { get; }\n        public CapacityDict<ushort, FixedUnorderedList<DeactivateKOTalentNetEventS2C>> DeactivateKOTalentNetEventsPerPlayer { get; }"
)

# Replace fields
content = content.replace(
    "private readonly ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> _deactivateSwapTalentNetEventsListPool;",
    "private readonly ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> _deactivateSwapTalentNetEventsListPool;\n        private readonly ConcurrentPool<FixedUnorderedList<CreateKOProjectileNetEventS2C>> _createKOProjectileNetEventsListPool;\n        private readonly ConcurrentPool<FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> _koProjectHitPlayerNetEventsListPool;\n        private readonly ConcurrentPool<FixedUnorderedList<DeactivateKOTalentNetEventS2C>> _deactivateKOTalentNetEventsListPool;"
)

# Replace init lists
content = content.replace(
    "DeactivateSwapTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(maxConcurrentPlayers);",
    "DeactivateSwapTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(maxConcurrentPlayers);\n            CreateKOProjectileNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>>(maxConcurrentPlayers);\n            KOProjectHitPlayerNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(maxConcurrentPlayers);\n            DeactivateKOTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(maxConcurrentPlayers);"
)

# Replace pool init
content = content.replace(
    "_deactivateSwapTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateSwapTalentNetEventS2C>(networkConfig.MaxCap.DestroySwapFieldNetEvents), maxConcurrentPlayers);",
    "_deactivateSwapTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateSwapTalentNetEventS2C>(networkConfig.MaxCap.DestroySwapFieldNetEvents), maxConcurrentPlayers);\n            _createKOProjectileNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateKOProjectileNetEventS2C>>(() => new FixedUnorderedList<CreateKOProjectileNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);\n            _koProjectHitPlayerNetEventsListPool = new ConcurrentPool<FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(() => new FixedUnorderedList<KOProjectHitPlayerNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);\n            _deactivateKOTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateKOTalentNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);"
)

# Replace StartSavingPlayerEvents
to_replace_start = """            if (!DeactivateSwapTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                DeactivateSwapTalentNetEventsPerPlayer.Add(playerId, _deactivateSwapTalentNetEventsListPool.Get());
            }"""
replacement_start = """            if (!DeactivateSwapTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                DeactivateSwapTalentNetEventsPerPlayer.Add(playerId, _deactivateSwapTalentNetEventsListPool.Get());
            }
            if (!CreateKOProjectileNetEventsPerPlayer.ContainsKey(playerId))
            {
                CreateKOProjectileNetEventsPerPlayer.Add(playerId, _createKOProjectileNetEventsListPool.Get());
            }
            if (!KOProjectHitPlayerNetEventsPerPlayer.ContainsKey(playerId))
            {
                KOProjectHitPlayerNetEventsPerPlayer.Add(playerId, _koProjectHitPlayerNetEventsListPool.Get());
            }
            if (!DeactivateKOTalentNetEventsPerPlayer.ContainsKey(playerId))
            {
                DeactivateKOTalentNetEventsPerPlayer.Add(playerId, _deactivateKOTalentNetEventsListPool.Get());
            }"""
content = content.replace(to_replace_start, replacement_start)

# Replace StopSavingPlayerEvents (returns to pool)
to_replace_stop = """            var deactivateSwapTalentNetEventsList = DeactivateSwapTalentNetEventsPerPlayer[playerId];
            deactivateSwapTalentNetEventsList.Clear();
            _deactivateSwapTalentNetEventsListPool.Return(deactivateSwapTalentNetEventsList);"""
replacement_stop = """            var deactivateSwapTalentNetEventsList = DeactivateSwapTalentNetEventsPerPlayer[playerId];
            deactivateSwapTalentNetEventsList.Clear();
            _deactivateSwapTalentNetEventsListPool.Return(deactivateSwapTalentNetEventsList);

            var createKOProjectileNetEventsList = CreateKOProjectileNetEventsPerPlayer[playerId];
            createKOProjectileNetEventsList.Clear();
            _createKOProjectileNetEventsListPool.Return(createKOProjectileNetEventsList);

            var koProjectHitPlayerNetEventsList = KOProjectHitPlayerNetEventsPerPlayer[playerId];
            koProjectHitPlayerNetEventsList.Clear();
            _koProjectHitPlayerNetEventsListPool.Return(koProjectHitPlayerNetEventsList);

            var deactivateKOTalentNetEventsList = DeactivateKOTalentNetEventsPerPlayer[playerId];
            deactivateKOTalentNetEventsList.Clear();
            _deactivateKOTalentNetEventsListPool.Return(deactivateKOTalentNetEventsList);"""
content = content.replace(to_replace_stop, replacement_stop)

# Replace StopSavingPlayerEvents (removes)
to_replace_remove = """            DeactivateSwapTalentNetEventsPerPlayer.Remove(playerId);"""
replacement_remove = """            DeactivateSwapTalentNetEventsPerPlayer.Remove(playerId);
            CreateKOProjectileNetEventsPerPlayer.Remove(playerId);
            KOProjectHitPlayerNetEventsPerPlayer.Remove(playerId);
            DeactivateKOTalentNetEventsPerPlayer.Remove(playerId);"""
content = content.replace(to_replace_remove, replacement_remove)

# Replace RemoveOldEvents
to_replace_old = """            if (DeactivateSwapTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateSwapTalentNetEvents))
            {
                for (int i = deactivateSwapTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateSwapTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateSwapTalentNetEvents.RemoveAt(i);
                    }
                }
            }"""
replacement_old = """            if (DeactivateSwapTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateSwapTalentNetEvents))
            {
                for (int i = deactivateSwapTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateSwapTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateSwapTalentNetEvents.RemoveAt(i);
                    }
                }
            }
            if (CreateKOProjectileNetEventsPerPlayer.TryGetValue(playerId, out var createKOProjectileNetEvents))
            {
                for (int i = createKOProjectileNetEvents.Count - 1; i >= 0; i--)
                {
                    if (createKOProjectileNetEvents[i].OccuredOnTick < tick)
                    {
                        createKOProjectileNetEvents.RemoveAt(i);
                    }
                }
            }
            if (KOProjectHitPlayerNetEventsPerPlayer.TryGetValue(playerId, out var koProjectHitPlayerNetEvents))
            {
                for (int i = koProjectHitPlayerNetEvents.Count - 1; i >= 0; i--)
                {
                    if (koProjectHitPlayerNetEvents[i].OccuredOnTick < tick)
                    {
                        koProjectHitPlayerNetEvents.RemoveAt(i);
                    }
                }
            }
            if (DeactivateKOTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateKOTalentNetEvents))
            {
                for (int i = deactivateKOTalentNetEvents.Count - 1; i >= 0; i--)
                {
                    if (deactivateKOTalentNetEvents[i].OccuredOnTick < tick)
                    {
                        deactivateKOTalentNetEvents.RemoveAt(i);
                    }
                }
            }"""
content = content.replace(to_replace_old, replacement_old)

# Replace AddEvents
to_replace_add = """        public void AddDeactivateSwapTalentNetEvent(int onTick, ushort casterPlayerId, ushort swapFieldId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateSwapTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.SwapFieldId = swapFieldId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }"""
replacement_add = """        public void AddDeactivateSwapTalentNetEvent(int onTick, ushort casterPlayerId, ushort swapFieldId, int talentCooldownEndTick)
        {
            foreach (var kvp in DeactivateSwapTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.SwapFieldId = swapFieldId;
                packet.TalentCooldownEndTick = talentCooldownEndTick;
            }
        }

        public void AddCreateKOProjectileNetEvent(int onTick, ushort projectileId, ushort casterPlayerId, System.Numerics.Vector2 position, System.Numerics.Vector2 velocity, float size)
        {
            foreach (var kvp in CreateKOProjectileNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.ProjectileId = projectileId;
                packet.CasterPlayerId = casterPlayerId;
                packet.Position = position;
                packet.Velocity = velocity;
                packet.Size = size;
            }
        }

        public void AddKOProjectHitPlayerNetEvent(int onTick, ushort projectileId, ushort hitPlayerId, System.Numerics.Vector2 hitPosition)
        {
            foreach (var kvp in KOProjectHitPlayerNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.ProjectileId = projectileId;
                packet.HitPlayerId = hitPlayerId;
                packet.HitPosition = hitPosition;
            }
        }

        public void AddDeactivateKOTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int cooldownEndTick)
        {
            foreach (var kvp in DeactivateKOTalentNetEventsPerPlayer)
            {
                ref var packet = ref kvp.Value.AddAndGet();
                packet.OccuredOnTick = onTick;
                packet.CasterPlayerId = casterPlayerId;
                packet.ProjectileId = projectileId;
                packet.CooldownEndTick = cooldownEndTick;
            }
        }"""
content = content.replace(to_replace_add, replacement_add)

with open('./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs', 'w') as f:
    f.write(content)
