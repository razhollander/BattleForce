using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Features.UI.Match.Scripts
{
    public class MatchPlayersUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _colorImage;
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthText;

        [Header("Talents")]
        [SerializeField] private Transform _talentsContainer;
        [SerializeField] private MatchPlayerTalentUIView _talentViewPrefab;
        // Optional: Map TalentType to Sprites via Inspector or ScriptableObject
        // For simplicity, I'll assume we can pass sprites or just use placeholders if no config is available yet.
        // But user didn't mention sprites source. I will just rely on prefab for now or setup generic ones.

        private readonly List<MatchPlayerTalentUIView> _talentViews = new List<MatchPlayerTalentUIView>();

        public void Setup(string playerName, Color color)
        {
            if (_nameText != null) _nameText.text = playerName;
            if (_colorImage != null) _colorImage.color = color;
            UpdateMoney(0);
        }

        public void UpdateMoney(int money)
        {
            if (_moneyText != null) _moneyText.text = money.ToString();
        }

        public void UpdateHealth(int current, int max)
        {
            if (_healthSlider != null)
            {
                _healthSlider.maxValue = max;
                _healthSlider.value = current;
            }
            if (_healthText != null)
            {
                _healthText.text = $"{current}/{max}";
            }
        }

        public void UpdateTalents(PlayerTalentsStateS2C talentsState)
        {
            // Ensure enough views
            int requiredCount = talentsState.Talents.Count;
            while (_talentViews.Count < requiredCount)
            {
                var view = Instantiate(_talentViewPrefab, _talentsContainer);
                view.Setup(null); // Pass icon if available
                _talentViews.Add(view);
            }

            // Hide extras
            for (int i = requiredCount; i < _talentViews.Count; i++)
            {
                _talentViews[i].gameObject.SetActive(false);
            }

            // Update views
            for (int i = 0; i < requiredCount; i++)
            {
                var view = _talentViews[i];
                view.gameObject.SetActive(true);
                var talent = talentsState.Talents.Get(i);
                bool isSelected = (i == talentsState.SelectedTalentIndex);
                view.UpdateView(talent, isSelected);
            }
        }
    }
}