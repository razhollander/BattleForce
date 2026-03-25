#!/bin/bash
sed -i 's/DashPulse = 5,/DashPulse = 5,\n        KO = 6/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/TalentType.cs
