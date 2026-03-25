#!/bin/bash
git checkout ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs

sed -i 's/public FixedUnorderedList<DeactivateKOTalentNetEventS2C> DeactivateKOTalentNetEvents;/public FixedUnorderedList<DeactivateKOTalentNetEventS2C> DeactivateKOTalentNetEvents;\n        public FixedUnorderedList<PerformDashPulseNetEventS2C> PerformDashPulseNetEvents;\n        public FixedUnorderedList<DeactivateDashPulseTalentNetEventS2C> DeactivateDashPulseTalentNetEvents;/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs

sed -i '/DeactivateKOTalentNetEvents = new FixedUnorderedList<DeactivateKOTalentNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents);/a\
            PerformDashPulseNetEvents = new FixedUnorderedList<PerformDashPulseNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents);\
            DeactivateDashPulseTalentNetEvents = new FixedUnorderedList<DeactivateDashPulseTalentNetEventS2C>(networkConfig.MaxCap.TalentSwitchNetEvents);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs

sed -i '/SerializeUnorderedList(writer, DeactivateKOTalentNetEvents);/a\
            SerializeUnorderedList(writer, PerformDashPulseNetEvents);\
            SerializeUnorderedList(writer, DeactivateDashPulseTalentNetEvents);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs

sed -i '/DeserializeUnorderedList(reader, ref DeactivateKOTalentNetEvents);/a\
            DeserializeUnorderedList(reader, ref PerformDashPulseNetEvents);\
            DeserializeUnorderedList(reader, ref DeactivateDashPulseTalentNetEvents);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs
