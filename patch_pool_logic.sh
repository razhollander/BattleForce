#!/bin/bash
sed -i '/if (!DeactivateSwapTalentNetEventsPerPlayer.ContainsKey(playerId))/i\
            }\
            if (!DeactivateSwapTalentNetEventsPerPlayer.ContainsKey(playerId))\
            {\
                DeactivateSwapTalentNetEventsPerPlayer.Add(playerId, _deactivateSwapTalentNetEventsListPool.Get());' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs
