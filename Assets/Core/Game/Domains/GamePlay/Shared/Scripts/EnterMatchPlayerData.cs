using System;

namespace Core.Game.Domains.GamePlay.Shared.Scripts
{
    [Serializable]
    public class EnterMatchPlayerData
    {
        public ushort Id;
        public string Name;
        public ushort TeamId;
    }
}