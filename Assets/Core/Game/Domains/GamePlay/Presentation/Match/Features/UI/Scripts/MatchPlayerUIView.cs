using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Features.Simple_Health_Bar.Scripts;
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
        [SerializeField] private GameObject _healthBarGameObject;
        [SerializeField] private TextMeshProUGUI _molesHitScoreText;
        [SerializeField] private GameObject _molesHitScoreContainer;
        [SerializeField] private TextMeshProUGUI _gatePassScoreText;
        [SerializeField] private GameObject _gatePassScoreContainer;
        [SerializeField]private CanvasGroup _canvasGroup;

        private TextMeshProUGUI _activeScoreText;

        [Header("Talents")]
        [SerializeField] private Transform _talentsContainer;
        [SerializeField] private MatchPlayerTalentUIView _talentViewPrefab;

        private MatchPlayerTalentUIView[] _talentViews;

        public void Setup(string playerName, Color color, int maxTalentsAmount)
        {
            _nameText.text = playerName;
            _spaceshipImage.color = color;
            UpdateMoney(0);
            CreateTalents(maxTalentsAmount);
        }

        private void CreateTalents(int maxTalentsAmount)
        {
            _talentViews = new MatchPlayerTalentUIView[maxTalentsAmount];

            for (int i = 0; i < maxTalentsAmount; i++)
            {
                var view = Instantiate(_talentViewPrefab, _talentsContainer);
                view.SetNoneTalent();
                _talentViews[i] = view;
            }
        }

        public void SetOpacity(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }

        public void UpdateMoney(int money)
        {
            _moneyText.text = money+"$";
        }

        public void SetHealth(int health, int maxHealth, CancellationToken cancellationToken)
        {
            _healthBar.UpdateBar(health, maxHealth, cancellationToken);
        }

        public void HideHealthBar()
        {
            _healthBarGameObject.SetActive(false);
        }
        
        public void ShowMolesHitScore(int molesHitScore)
        {
            HideHealthBar();
            _molesHitScoreContainer.SetActive(true);
            _activeScoreText = _molesHitScoreText;
            UpdateMolesHitScore(molesHitScore);
        }

        public void ShowGatePassScore(int gatePassScore)
        {
            HideHealthBar();
            _gatePassScoreContainer.SetActive(true);
            _activeScoreText = _gatePassScoreText;
            UpdateMolesHitScore(gatePassScore);
        }

        public void UpdateMolesHitScore(int score)
        {
            _activeScoreText.text = score.ToString();
        }

        public void UpdateTalents(TalentVisualData[] talents)
        {
            for (int i = 0; i < _talentViews.Length; i++)
            {
                var view = _talentViews[i];

                if (i > talents.Length - 1)
                {
                    view.SetNoneTalent();
                }
                else
                {
                    view.SetTalent(talents[i]);
                }
            }
        }

        public void UpdateTalentCooldown(int talentIndex, float maxCooldown, float cooldownLeft, bool isOnCooldown)
        {
            _talentViews[talentIndex].UpdateCooldown(maxCooldown, cooldownLeft, isOnCooldown);
        }

        public void SetSelectedTalent(int selectedTalentIndex)
        {
            for (int i = 0; i < _talentViews.Length; i++)
            {
                var talentView = _talentViews[i];
                talentView.SetIsSelected(selectedTalentIndex == i);
            }
        }

        public void UpdateTalentStocks(int talentIndex, int stockAmount)
        {
            _talentViews[talentIndex].SetStocksAmount(stockAmount);
        }
    }
}