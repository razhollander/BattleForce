#!/bin/bash
sed -i 's/SwapField = 11/SwapField = 11,\n        KOProjectile = 12/g' ./Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/Physics/PhysicsBodyType.cs
