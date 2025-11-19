using System;
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

    public struct PlayerInputC2S : IComparable<PlayerInputC2S>
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
            writer.Put(AimDirection);
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
            AimDirection = reader.GetFloat();
        }
        
        public int CompareTo(PlayerInputC2S other)
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
    
    [Flags]
    public enum PacketTypeC2S : byte
    {
        PlayerInput = 1 << 1,
        Spawn = 1 << 2,
        Shoot = 1 << 3
    }
}