using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public class MatchMakingUiView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _publicIpAddressText;
        [SerializeField] private TextMeshProUGUI _localIpAddressText;
        [SerializeField] private TextMeshProUGUI _portText;

        public void Setup(string publicIpAddress,string localIpAddress, string port)
        {
            _publicIpAddressText.text = "Public IP:" + publicIpAddress;
            _localIpAddressText.text = "Local IP:" + localIpAddress;
            _portText.text = "Port: "+ port;
        }
    }
}