using TMPro;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.MatchMakingUI.Scripts.Mvcs
{
    public class MatchMakingUiView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _ipAddressText;

        public void SetIpAddress(string ip)
        {
            if (_ipAddressText != null)
            {
                _ipAddressText.text = ip;
            }
        }
    }
}