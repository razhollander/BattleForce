#!/bin/bash
git checkout ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs
sed -i 's/public CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerPlayer { get; }/public CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> DeactivateSwapTalentNetEventsPerPlayer { get; }\n        public CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>> CreateKOProjectileNetEventsPerPlayer { get; }\n        public CapacityDict<ushort, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> KOProjectHitPlayerNetEventsPerPlayer { get; }\n        public CapacityDict<ushort, FixedUnorderedList<DeactivateKOTalentNetEventS2C>> DeactivateKOTalentNetEventsPerPlayer { get; }/g' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/private readonly ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>> _deactivateSwapTalentNetEventsListPool;/a\
        private readonly ConcurrentPool<FixedUnorderedList<CreateKOProjectileNetEventS2C>> _createKOProjectileNetEventsListPool;\
        private readonly ConcurrentPool<FixedUnorderedList<KOProjectHitPlayerNetEventS2C>> _koProjectHitPlayerNetEventsListPool;\
        private readonly ConcurrentPool<FixedUnorderedList<DeactivateKOTalentNetEventS2C>> _deactivateKOTalentNetEventsListPool;' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/DeactivateSwapTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>(maxConcurrentPlayers);/a\
            CreateKOProjectileNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<CreateKOProjectileNetEventS2C>>(maxConcurrentPlayers);\
            KOProjectHitPlayerNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(maxConcurrentPlayers);\
            DeactivateKOTalentNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(maxConcurrentPlayers);' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/_deactivateSwapTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateSwapTalentNetEventS2C>>/a\
            _createKOProjectileNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateKOProjectileNetEventS2C>>(() => new FixedUnorderedList<CreateKOProjectileNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);\
            _koProjectHitPlayerNetEventsListPool = new ConcurrentPool<FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(() => new FixedUnorderedList<KOProjectHitPlayerNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);\
            _deactivateKOTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateKOTalentNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/DeactivateSwapTalentNetEventsPerPlayer.Add(playerId, _deactivateSwapTalentNetEventsListPool.Get());/a\
            }\
            if (!CreateKOProjectileNetEventsPerPlayer.ContainsKey(playerId))\
            {\
                CreateKOProjectileNetEventsPerPlayer.Add(playerId, _createKOProjectileNetEventsListPool.Get());\
            }\
            if (!KOProjectHitPlayerNetEventsPerPlayer.ContainsKey(playerId))\
            {\
                KOProjectHitPlayerNetEventsPerPlayer.Add(playerId, _koProjectHitPlayerNetEventsListPool.Get());\
            }\
            if (!DeactivateKOTalentNetEventsPerPlayer.ContainsKey(playerId))\
            {\
                DeactivateKOTalentNetEventsPerPlayer.Add(playerId, _deactivateKOTalentNetEventsListPool.Get());' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/var deactivateSwapTalentNetEventsList = DeactivateSwapTalentNetEventsPerPlayer\[playerId\];/a\
            var createKOProjectileNetEventsList = CreateKOProjectileNetEventsPerPlayer[playerId];\
            createKOProjectileNetEventsList.Clear();\
            _createKOProjectileNetEventsListPool.Return(createKOProjectileNetEventsList);\
            var koProjectHitPlayerNetEventsList = KOProjectHitPlayerNetEventsPerPlayer[playerId];\
            koProjectHitPlayerNetEventsList.Clear();\
            _koProjectHitPlayerNetEventsListPool.Return(koProjectHitPlayerNetEventsList);\
            var deactivateKOTalentNetEventsList = DeactivateKOTalentNetEventsPerPlayer[playerId];\
            deactivateKOTalentNetEventsList.Clear();\
            _deactivateKOTalentNetEventsListPool.Return(deactivateKOTalentNetEventsList);' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/DeactivateSwapTalentNetEventsPerPlayer.Remove(playerId);/a\
            CreateKOProjectileNetEventsPerPlayer.Remove(playerId);\
            KOProjectHitPlayerNetEventsPerPlayer.Remove(playerId);\
            DeactivateKOTalentNetEventsPerPlayer.Remove(playerId);' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/if (DeactivateSwapTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateSwapTalentNetEvents))/i\
            if (CreateKOProjectileNetEventsPerPlayer.TryGetValue(playerId, out var createKOProjectileNetEvents))\
            {\
                for (int i = createKOProjectileNetEvents.Count - 1; i >= 0; i--)\
                {\
                    if (createKOProjectileNetEvents[i].OccuredOnTick < tick)\
                    {\
                        createKOProjectileNetEvents.RemoveAt(i);\
                    }\
                }\
            }\
            if (KOProjectHitPlayerNetEventsPerPlayer.TryGetValue(playerId, out var koProjectHitPlayerNetEvents))\
            {\
                for (int i = koProjectHitPlayerNetEvents.Count - 1; i >= 0; i--)\
                {\
                    if (koProjectHitPlayerNetEvents[i].OccuredOnTick < tick)\
                    {\
                        koProjectHitPlayerNetEvents.RemoveAt(i);\
                    }\
                }\
            }\
            if (DeactivateKOTalentNetEventsPerPlayer.TryGetValue(playerId, out var deactivateKOTalentNetEvents))\
            {\
                for (int i = deactivateKOTalentNetEvents.Count - 1; i >= 0; i--)\
                {\
                    if (deactivateKOTalentNetEvents[i].OccuredOnTick < tick)\
                    {\
                        deactivateKOTalentNetEvents.RemoveAt(i);\
                    }\
                }\
            }' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/packet.TalentCooldownEndTick = talentCooldownEndTick;/a\
            }\
        }\
\
        public void AddCreateKOProjectileNetEvent(int onTick, ushort projectileId, ushort casterPlayerId, System.Numerics.Vector2 position, System.Numerics.Vector2 velocity, float size)\
        {\
            foreach (var kvp in CreateKOProjectileNetEventsPerPlayer)\
            {\
                ref var packet = ref kvp.Value.AddAndGet();\
                packet.OccuredOnTick = onTick;\
                packet.ProjectileId = projectileId;\
                packet.CasterPlayerId = casterPlayerId;\
                packet.Position = position;\
                packet.Velocity = velocity;\
                packet.Size = size;\
            }\
        }\
\
        public void AddKOProjectHitPlayerNetEvent(int onTick, ushort projectileId, ushort hitPlayerId, System.Numerics.Vector2 hitPosition)\
        {\
            foreach (var kvp in KOProjectHitPlayerNetEventsPerPlayer)\
            {\
                ref var packet = ref kvp.Value.AddAndGet();\
                packet.OccuredOnTick = onTick;\
                packet.ProjectileId = projectileId;\
                packet.HitPlayerId = hitPlayerId;\
                packet.HitPosition = hitPosition;\
            }\
        }\
\
        public void AddDeactivateKOTalentNetEvent(int onTick, ushort casterPlayerId, ushort projectileId, int cooldownEndTick)\
        {\
            foreach (var kvp in DeactivateKOTalentNetEventsPerPlayer)\
            {\
                ref var packet = ref kvp.Value.AddAndGet();\
                packet.OccuredOnTick = onTick;\
                packet.CasterPlayerId = casterPlayerId;\
                packet.ProjectileId = projectileId;\
                packet.CooldownEndTick = cooldownEndTick;' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs
