#!/bin/bash
sed -i '/if (!CreateSwapFieldNetEventsPerPlayer.ContainsKey(playerId))/i\
            if (!DeactivateKOTalentNetEventsPerPlayer.ContainsKey(playerId))\
            {\
                DeactivateKOTalentNetEventsPerPlayer.Add(playerId, _deactivateKOTalentNetEventsListPool.Get());\
            }' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs

sed -i '/if (!DeactivateSwapTalentNetEventsPerPlayer.ContainsKey(playerId))/d' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs
sed -i '/DeactivateSwapTalentNetEventsPerPlayer.Add(playerId, _deactivateSwapTalentNetEventsListPool.Get());/d' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs
sed -i '/if (!CreateKOProjectileNetEventsPerPlayer.ContainsKey(playerId))/i\
            if (!DeactivateSwapTalentNetEventsPerPlayer.ContainsKey(playerId))\
            {\
                DeactivateSwapTalentNetEventsPerPlayer.Add(playerId, _deactivateSwapTalentNetEventsListPool.Get());\
            }' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs
