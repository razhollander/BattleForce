#!/bin/bash
sed -i '/DeserializedCreateSwapFieldNetEvents(reader);/a\
            DeserializedCreateKOProjectileNetEvents(reader);\
            DeserializedKOProjectHitPlayerNetEvents(reader);\
            DeserializedDeactivateKOTalentNetEvents(reader);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs

sed -i '/private void DeserializedCreateSwapFieldNetEvents(NetDataReader reader)/i\
        private void DeserializedCreateKOProjectileNetEvents(NetDataReader reader)\
        {\
            CreateKOProjectileNetEvents.Clear();\
            var count = reader.GetByte();\
            for (var i = 0; i < count; i++)\
            {\
                ref var evt = ref CreateKOProjectileNetEvents.AddAndGet();\
                evt.Deserialize(reader);\
            }\
        }\
\
        private void DeserializedKOProjectHitPlayerNetEvents(NetDataReader reader)\
        {\
            KOProjectHitPlayerNetEvents.Clear();\
            var count = reader.GetByte();\
            for (var i = 0; i < count; i++)\
            {\
                ref var evt = ref KOProjectHitPlayerNetEvents.AddAndGet();\
                evt.Deserialize(reader);\
            }\
        }\
\
        private void DeserializedDeactivateKOTalentNetEvents(NetDataReader reader)\
        {\
            DeactivateKOTalentNetEvents.Clear();\
            var count = reader.GetByte();\
            for (var i = 0; i < count; i++)\
            {\
                ref var evt = ref DeactivateKOTalentNetEvents.AddAndGet();\
                evt.Deserialize(reader);\
            }\
        }' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs
