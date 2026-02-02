using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class StageEndedUiView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _winningTeamText;
        [SerializeField] private TextMeshProUGUI _jemsText;
        [SerializeField] private GameObject _panel;

        public void Show(int winningTeamId, Color teamColor, Dictionary<ushort, int> totalJems)
        {
            _panel.SetActive(true);
            _winningTeamText.text = $"Team {winningTeamId} Wins!";
            _winningTeamText.color = teamColor;

            var jemsInfo = "Jems:\n";
            foreach(var kvp in totalJems)
            {
                jemsInfo += $"Team {kvp.Key}: {kvp.Value}\n";
            }
            _jemsText.text = jemsInfo;
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }
    }
}
