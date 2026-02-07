using System;

namespace Core.Game.Domains.GamePlay.Shared.Scripts
{
    [Serializable]
    public class EnterMatchPlayerData
    {
        public ushort Id;
        public string Name;
        public ushort TeamId;

        public EnterMatchPlayerData(ushort id, string name, ushort teamId)
        {
            Id = id;
            Name = name;
            TeamId = teamId;
        }
    }
}