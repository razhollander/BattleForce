using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public class MatchMakingUiView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _ipAddressText;
        [SerializeField] private TextMeshProUGUI _portText;

        public void Setup(string ipAddress, string port)
        {
            _ipAddressText.text = "IP Address:" + ipAddress;
            _portText.text = "Port: "+ port;
        }
    }
}