using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts
{
    [CreateAssetMenu(fileName = "GalacticStarsVisualData", menuName = "BF/Presentation/Galactic Stars Visual Data")]
    public class GalacticStarsVisualData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<int, GalacticStarVisualData> _visualDataPerTeamId;

        public GalacticStarVisualData GetByTeamId(int teamId)
        {
            return _visualDataPerTeamId[teamId];
        }
    }
}
