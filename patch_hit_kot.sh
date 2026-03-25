#!/bin/bash
sed -i 's/public ushort KoProjectileId;/public ushort ProjectileId;\n        public ushort HitPlayerId;\n        public Vector2 HitPosition;/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs
sed -i '/public Vector2 HitPoint;/d' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs

sed -i 's/public KOProjectHitPlayerNetEventS2C(int occuredOnTick, ushort koProjectileId, Vector2 hitPoint)/public KOProjectHitPlayerNetEventS2C(int occuredOnTick, ushort projectileId, ushort hitPlayerId, Vector2 hitPosition)/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs

sed -i 's/KoProjectileId = koProjectileId;/ProjectileId = projectileId;\n            HitPlayerId = hitPlayerId;\n            HitPosition = hitPosition;/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs
sed -i '/HitPoint = hitPoint;/d' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs

sed -i 's/writer.Put(KoProjectileId);/writer.Put(ProjectileId);\n            writer.Put((byte)HitPlayerId);/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs
sed -i 's/writer.PutVector2Quantized(HitPoint);/writer.PutVector2Quantized(HitPosition);/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs

sed -i 's/KoProjectileId = reader.GetUShort();/ProjectileId = reader.GetUShort();\n            HitPlayerId = reader.GetByte();/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs
sed -i 's/HitPoint = reader.GetVector2Quantized();/HitPosition = reader.GetVector2Quantized();/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/KOProjectHitPlayerNetEventS2C.cs
