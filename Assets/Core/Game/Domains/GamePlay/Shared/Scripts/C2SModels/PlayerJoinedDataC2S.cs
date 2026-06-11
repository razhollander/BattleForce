using System;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.C2SModels
{
    [Serializable]
    public struct PlayerJoinedDataC2S
    {
        public string PlayerName;
        public bool IsGamepad;
        public int InputDeviceId;

        public PlayerJoinedDataC2S(string playerName, bool isGamepad, int inputDeviceId)
        {
            PlayerName = playerName;
            IsGamepad = isGamepad;
            InputDeviceId = inputDeviceId;
        }        
    }
}

