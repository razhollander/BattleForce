#!/bin/bash
sed -i '/SerializedCreateSwapFieldNetEvents(writer);/a\
            SerializedCreateKOProjectileNetEvents(writer);\
            SerializedKOProjectHitPlayerNetEvents(writer);\
            SerializedDeactivateKOTalentNetEvents(writer);' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs

sed -i '/private void SerializedCreateSwapFieldNetEvents(NetDataWriter writer)/i\
        private void SerializedCreateKOProjectileNetEvents(NetDataWriter writer)\
        {\
            var count = CreateKOProjectileNetEvents.Count;\
            writer.Put((byte)count);\
            foreach (var evt in CreateKOProjectileNetEvents.AsSpan())\
            {\
                evt.Serialize(writer);\
            }\
        }\
\
        private void SerializedKOProjectHitPlayerNetEvents(NetDataWriter writer)\
        {\
            var count = KOProjectHitPlayerNetEvents.Count;\
            writer.Put((byte)count);\
            foreach (var evt in KOProjectHitPlayerNetEvents.AsSpan())\
            {\
                evt.Serialize(writer);\
            }\
        }\
\
        private void SerializedDeactivateKOTalentNetEvents(NetDataWriter writer)\
        {\
            var count = DeactivateKOTalentNetEvents.Count;\
            writer.Put((byte)count);\
            foreach (var evt in DeactivateKOTalentNetEvents.AsSpan())\
            {\
                evt.Serialize(writer);\
            }\
        }' ./Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs
