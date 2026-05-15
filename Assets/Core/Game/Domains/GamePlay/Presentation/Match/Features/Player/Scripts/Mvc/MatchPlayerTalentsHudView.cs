using Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Player.Scripts.Mvc
{
    public class MatchPlayerTalentsHudView : MonoBehaviour
    {
        private const string TANELT_A_SELECTED_ANIMTION_NAME = "TaneltASelected";
        private const string TANELT_B_SELECTED_ANIMTION_NAME = "TaneltBSelected";
        private const string TANELT_C_SELECTED_ANIMTION_NAME = "TaneltCSelected";
        
        [SerializeField] private Animation _animation;
        [SerializeField] private float _secondsOfCrossFadeAnimation=0.3f;
        [SerializeField] private MatchPlayerTalentHudView[] _talentViews;

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
        
        public void SelectTalent(int talentIndex)
        {
            switch (talentIndex)
            {
                case 0:
                    SelectTalentA();

                    break;
                case 1:
                    SelectTalentB();

                    break;
                case 2:
                    SelectTalentC();

                    break;
            }
        }
        
        private void SelectTalentA()
        {
            _animation.CrossFade(TANELT_A_SELECTED_ANIMTION_NAME, _secondsOfCrossFadeAnimation);
        }
        private void SelectTalentB()
        {
            _animation.CrossFade(TANELT_B_SELECTED_ANIMTION_NAME, _secondsOfCrossFadeAnimation);
        }
        private void SelectTalentC()
        {
            _animation.CrossFade(TANELT_C_SELECTED_ANIMTION_NAME, _secondsOfCrossFadeAnimation);
        }
    }
}
