#!/bin/bash
sed -i '/_fullTickPacket.CreateSwapFieldNetEvents = _netEventsDataService.CreateSwapFieldNetEventsPerPlayer\[playerId\];/a\
                _fullTickPacket.CreateKOProjectileNetEvents = _netEventsDataService.CreateKOProjectileNetEventsPerPlayer[playerId];\
                _fullTickPacket.KOProjectHitPlayerNetEvents = _netEventsDataService.KOProjectHitPlayerNetEventsPerPlayer[playerId];\
                _fullTickPacket.DeactivateKOTalentNetEvents = _netEventsDataService.DeactivateKOTalentNetEventsPerPlayer[playerId];' ./Assets/Core/Game/Domains/GamePlay/Simulation/Match/Scripts/NetworkManager/TickHandlers/ServerMatchNetworkTickProcessor.cs
