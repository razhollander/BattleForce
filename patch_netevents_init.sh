#!/bin/bash
sed -i '/_deactivateSwapTalentNetEventsListPool = new ConcurrentPool/a\
            _createKOProjectileNetEventsListPool = new ConcurrentPool<FixedUnorderedList<CreateKOProjectileNetEventS2C>>(() => new FixedUnorderedList<CreateKOProjectileNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);\
            _koProjectHitPlayerNetEventsListPool = new ConcurrentPool<FixedUnorderedList<KOProjectHitPlayerNetEventS2C>>(() => new FixedUnorderedList<KOProjectHitPlayerNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);\
            _deactivateKOTalentNetEventsListPool = new ConcurrentPool<FixedUnorderedList<DeactivateKOTalentNetEventS2C>>(() => new FixedUnorderedList<DeactivateKOTalentNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents), maxConcurrentPlayers);' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs
