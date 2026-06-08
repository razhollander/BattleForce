using System;
using Core.Scripts.Utils.CustomCollections;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.C2SModels.Packets
{
    /// <summary>
    /// The main packet sent to the server, containing inputs for all local players.
    /// </summary>
    public struct MatchPlayersInputPacketC2S : INetSerializable, IComparable<MatchPlayersInputPacketC2S> // todo change to class
    {
        // todo: add inputs from client unprocessed ticks
        public int Tick;
        public int HeighestProcessedTickFromServer;
        public FixedUnorderedList<MatchLocalPlayerInputDataC2S> PlayerInputs;

        public MatchPlayersInputPacketC2S(int maxPlayersInputs)
        {
            Tick = 0;
            HeighestProcessedTickFromServer = 0;
            PlayerInputs = new FixedUnorderedList<MatchLocalPlayerInputDataC2S>(maxPlayersInputs);
        }

        public void CopyFrom(MatchPlayersInputPacketC2S other)
        {
            Tick = other.Tick;
            HeighestProcessedTickFromServer = other.HeighestProcessedTickFromServer;
            PlayerInputs.Clear();

            foreach (var otherPlayerInput in other.PlayerInputs.AsSpan())
            {
                ref var playerInput = ref PlayerInputs.AddAndGet();
                playerInput = otherPlayerInput;
            }
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.Put(HeighestProcessedTickFromServer);
            
            byte playerCount = (byte) (PlayerInputs.Count);
            writer.Put(playerCount);

            for (int i = 0; i < playerCount; i++)
            {
                PlayerInputs[i].Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            HeighestProcessedTickFromServer = reader.GetInt();

            byte playerCount = reader.GetByte();
            PlayerInputs.Clear();

            for (int i = 0; i < playerCount; i++)
            {
                ref var playerInputData = ref PlayerInputs.AddAndGet();
                playerInputData.Deserialize(reader);
            }
        }

        public int CompareTo(MatchPlayersInputPacketC2S other)
        {
            return Tick.CompareTo(other.Tick);
        }
    }
}