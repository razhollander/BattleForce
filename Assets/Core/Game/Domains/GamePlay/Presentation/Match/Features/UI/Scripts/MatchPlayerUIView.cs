using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Features.Simple_Health_Bar.Scripts;
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts
{
    public class MatchPlayerUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _spaceshipImage;
        [SerializeField] private Image _equipmentImage;
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private SimpleHealthBar _healthBar;

        [Header("Talents")]
        [SerializeField] private Transform _talentsContainer;
        [SerializeField] private MatchPlayerTalentUIView _talentViewPrefab;

        private readonly List<MatchPlayerTalentUIView> _talentViews = new List<MatchPlayerTalentUIView>();

        public void Setup(string playerName, Color color)
        {
            _nameText.text = playerName;
            _spaceshipImage.color = color;
            UpdateMoney(0);
        }

        public void UpdateMoney(int money)
        {
            _moneyText.text = money+"$";
        }

        public void SetHealth(int health, int maxHealth)
        {
            _healthBar.UpdateBar(health, maxHealth);
        }

        public void HideHealthBar()
        {
            _healthBar.gameObject.SetActive(false);
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