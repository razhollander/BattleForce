using System.Threading;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using Core.Scripts.Extensions;
using Core.Scripts.Utils;
using UnityEngine;
using DG.Tweening;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerTalentsHudView : MonoBehaviour
    {
        [Header("Tween Settings")]
        [SerializeField] private float _scaleTweenDuration = 0.3f;
        [SerializeField] private Ease _scaleTweenEase = Ease.OutQuad;
        [SerializeField] private float _unselectedScaleX = 0.6f;
        [SerializeField] private float _selectedScaleY = 1f;
        
        [Header("References")]
        [SerializeField] private MatchPlayerTalentHudView[] _talentViews;
        [SerializeField] private Transform[] _talentViewsPivots;

        private CancellationTokenSource _selectTalentCancellationToken;
        
        public void UpdateTalentCooldown(int talentIndex, float maxCooldown, float cooldownLeft, bool isOnCooldown)
        {
            if (_talentViews != null && talentIndex < _talentViews.Length)
            {
                _talentViews[talentIndex].UpdateCooldown(maxCooldown, cooldownLeft, isOnCooldown);
            }
        }

        public void UpdateTalentStocks(int talentIndex, int stockAmount)
        {
            if (_talentViews != null && talentIndex < _talentViews.Length)
            {
                _talentViews[talentIndex].SetStocksAmount(stockAmount);
            }
        }
        
        public void UpdateTalents(TalentVisualData[] talents)
        {
            if (_talentViews == null) return;
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
        
        public void SelectTalent(int talentIndex, CancellationToken cancellationToken)
        {
            if (_selectTalentCancellationToken != null)
            {
                _selectTalentCancellationToken.Cancel();
                _selectTalentCancellationToken.Dispose();
            }
            
            _selectTalentCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            for (int i = 0; i < _talentViewsPivots.Length; i++)
            {
                var view = _talentViewsPivots[i];
                var isSelected = i == talentIndex;
                var targetScaleValue = isSelected ? _selectedScaleY : _unselectedScaleX;
                var targetScale = new Vector3(targetScaleValue, targetScaleValue, 1f);
                view.DOScale(targetScale, _scaleTweenDuration)
                    .SetEase(_scaleTweenEase)
                    .WithCancellationSafe(_selectTalentCancellationToken.Token).Forget();
            }
        }
    }
}