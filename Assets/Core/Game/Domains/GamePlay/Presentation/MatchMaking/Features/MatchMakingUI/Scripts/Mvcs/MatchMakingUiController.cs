using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public class MatchMakingUiController : IMatchMakingUiController
    {
        private readonly MatchMakingUiView _view;

        public MatchMakingUiController(MatchMakingUiView view)
        {
            _view = view;
        }

        public void InitEntryPoint(string ipAddress, int port, bool isHost)
        {
            InitEntryPointAsync(ipAddress, port, isHost).Forget();
        }

        private async Awaitable InitEntryPointAsync(string ipAddress, int port, bool isHost)
        {
            _view.Setup(ipAddress, "", port.ToString());
            var publicIp = await NetworkUtils.GetPublicIpAddress();
            _view.Setup(isHost ? publicIp : ipAddress, NetworkUtils.GetLocalIPAddress(), port.ToString());
        }
    }
}