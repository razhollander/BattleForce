using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Game.Domains.GamePlay.Shared;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public class TeamsBoardUIController : ITeamsBoardUIController
    {
        private readonly TeamsBoardContainerView _view;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private readonly PresentationGamePlayConfig _presentationGamePlayConfig;
        private readonly List<TeamBoardUIView> _teamViews = new List<TeamBoardUIView>();

        public TeamsBoardUIController(TeamsBoardContainerView view, SharedGamePlayConfig sharedGamePlayConfig, PresentationGamePlayConfig presentationGamePlayConfig)
        {
            _view = view;
            _sharedGamePlayConfig = sharedGamePlayConfig;
            _presentationGamePlayConfig = presentationGamePlayConfig;
        }

        public void InitEntryPoint()
        {
            // Assuming Team IDs are 1 to MaxTeamsAmount.
            // Adjust if logic differs (e.g. from MatchDataService).
            // But since we want to show ALL teams even if no players joined?
            // Usually we only show active teams.
            // But the prompt says "X= amount of teams".
            // I'll create for all possible teams.

            for (ushort i = 1; i <= _sharedGamePlayConfig.MaxTeamsAmount; i++)
            {
                var teamView = Object.Instantiate(_view.Prefab, _view.Container);
                if (_presentationGamePlayConfig.ColorPerTeamId.TryGetValue(i, out var color))
                {
                    teamView.Setup(color);
                }
                else
                {
                    teamView.Setup(Color.white);
                }
                _teamViews.Add(teamView);
            }
        }

        public void UpdateTeamGems(ushort teamId, int gems)
        {
            // Map teamId to index. Assuming teamId starts at 1.
            int index = teamId - 1;
            if (index >= 0 && index < _teamViews.Count)
            {
                _teamViews[index].UpdateGems(gems);
            }
        }
    }
}
