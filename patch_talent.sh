#!/bin/bash
sed -i '/KO = 6/d' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/TalentType.cs
sed -i '/DashPulse = 5,/a\
        KO = 6,' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/TalentType.cs
