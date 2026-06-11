using System;
using Core.Game.Domains.GamePlay.Presentation.Scripts.InputBeingUsed;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.ChooseNetworkRole.Scripts
{
    [Serializable]
    public class PlayerJoinedModel
    {
        public string PlayerName;
        public SupportedInputType PlayerInputType;
        public int InputDeviceId;

        public PlayerJoinedModel(string playerName, SupportedInputType playerInputType, int inputDeviceId)
        {
            PlayerName = playerName;
            PlayerInputType = playerInputType;
            InputDeviceId = inputDeviceId;
        }
    }
}