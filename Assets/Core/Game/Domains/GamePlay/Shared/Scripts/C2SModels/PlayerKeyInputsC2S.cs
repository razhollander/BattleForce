using System;
using Core.Game.Domains.GamePlay.Shared.C2SModels;
using Core.Game.Domains.GamePlay.Shared.Extensions;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.ClientToServerModels
{
    public struct NetPacketSerializable : INetSerializable
    {
        public PacketTypeC2S PacketType;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)PacketType);
        }

        public void Deserialize(NetDataReader reader)
        {
            PacketType = (PacketTypeC2S)reader.GetByte();
        }
    }

    public struct PlayerKeyInputsC2S : IComparable<PlayerKeyInputsC2S>
    {
        public NetPacketSerializable NetPacketSerializable;
        public int Tick;
        public int PlayerId;
        public byte InputKeysCount;
        public InputKeyType[] InputKeys; // todo: send also the inputs from previous ticks
        public float AimDirection;

        public void Serialize(NetDataWriter writer)
        {
            NetPacketSerializable.Serialize(writer);
            writer.Put(Tick);
            writer.Put((byte)PlayerId);
            writer.Put(InputKeysCount);
            for (int i = 0; i < InputKeysCount; i++)
            {
                writer.Put((byte)InputKeys[i]); 
            }
            writer.PutFloat16(AimDirection);
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            PlayerId = reader.GetByte();
            InputKeysCount = reader.GetByte();
            InputKeys = new InputKeyType[InputKeysCount];
            for (int i = 0; i < InputKeysCount; i++)
            {
                InputKeys[i] = (InputKeyType)reader.GetByte();
            }
            AimDirection = reader.GetFloat16();
        }
        
        public int CompareTo(PlayerKeyInputsC2S other)
        {
            return Tick.CompareTo(other.Tick);
        }
    }
    
    [Flags]
    public enum InputKeyType : byte
    {
        LeftPressed = 1 << 1,
        RightPressed = 1 << 2,
        ShootPressed = 1 << 3,
        // Special1 = 1 << 4,
        // Special2 = 1 << 5,
        // Force = 1 << 5,
    }
}