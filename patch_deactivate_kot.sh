#!/bin/bash
sed -i '/public ushort CasterPlayerId;/a\
        public int CooldownEndTick;' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs

sed -i 's/public DeactivateKOTalentNetEventS2C(int occuredOnTick, ushort koProjectileId, ushort casterPlayerId)/public DeactivateKOTalentNetEventS2C(int occuredOnTick, ushort koProjectileId, ushort casterPlayerId, int cooldownEndTick)/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs

sed -i '/CasterPlayerId = casterPlayerId;/a\
            CooldownEndTick = cooldownEndTick;' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs

sed -i '/writer.Put((byte)CasterPlayerId);/a\
            writer.Put(CooldownEndTick);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs

sed -i '/CasterPlayerId = reader.GetByte();/a\
            CooldownEndTick = reader.GetInt();' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs

sed -i 's/public ushort KoProjectileId;/public ushort ProjectileId;/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs
sed -i 's/KoProjectileId = koProjectileId;/ProjectileId = koProjectileId;/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs
sed -i 's/writer.Put(KoProjectileId);/writer.Put(ProjectileId);/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs
sed -i 's/KoProjectileId = reader.GetUShort();/ProjectileId = reader.GetUShort();/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/DeactivateKOTalentNetEventS2C.cs
