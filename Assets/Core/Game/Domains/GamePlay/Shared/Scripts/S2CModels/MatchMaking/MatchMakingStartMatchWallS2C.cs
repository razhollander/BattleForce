using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.MatchMaking
{
    public class MatchMakingStartMatchWallS2C
    {
        public bool IsEnabled = true;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsEnabled);
        }

        public void Deserialize(NetDataReader reader)
        {
            IsEnabled = reader.GetBool();
        }
    }
}