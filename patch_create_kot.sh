#!/bin/bash
sed -i '/public TalentKOProjectileS2C KoProjectile;/d' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs
sed -i '/public ushort CasterPlayerId;/i\
        public ushort ProjectileId;\
        public System.Numerics.Vector2 Position;\
        public System.Numerics.Vector2 Velocity;\
        public float Size;' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs

sed -i 's/public CreateKOProjectileNetEventS2C(int occuredOnTick, TalentKOProjectileS2C koProjectile, ushort casterPlayerId)/public CreateKOProjectileNetEventS2C(int occuredOnTick, ushort projectileId, ushort casterPlayerId, System.Numerics.Vector2 position, System.Numerics.Vector2 velocity, float size)/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs

sed -i 's/KoProjectile = koProjectile;/ProjectileId = projectileId;\n            Position = position;\n            Velocity = velocity;\n            Size = size;/g' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs

sed -i '/writer.Put(KoProjectile);/d' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs
sed -i '/writer.Put((byte)CasterPlayerId);/i\
            writer.Put(ProjectileId);\
            Core.Game.Domains.GamePlay.Shared.Extensions.NetDataWriterExtensions.PutVector2Quantized(writer, Position);\
            Core.Game.Domains.GamePlay.Shared.Extensions.NetDataWriterExtensions.PutVector2Quantized(writer, Velocity);\
            Core.Game.Domains.GamePlay.Shared.Extensions.NetDataWriterExtensions.PutFloat16(writer, Size);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs

sed -i '/KoProjectile.Deserialize(reader);/d' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs
sed -i '/CasterPlayerId = reader.GetByte();/i\
            ProjectileId = reader.GetUShort();\
            Position = Core.Game.Domains.GamePlay.Shared.Extensions.NetDataReaderExtensions.GetVector2Quantized(reader);\
            Velocity = Core.Game.Domains.GamePlay.Shared.Extensions.NetDataReaderExtensions.GetVector2Quantized(reader);\
            Size = Core.Game.Domains.GamePlay.Shared.Extensions.NetDataReaderExtensions.GetFloat16(reader);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateKOProjectileNetEventS2C.cs
