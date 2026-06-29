using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts
{
    [CreateAssetMenu(fileName = "GalacticStarsVisualData", menuName = "BF/Presentation/Galactic Stars Visual Data")]
    public class GalacticStarsVisualData : ScriptableObject
    {
        [SerializeField] private GalacticStarVisualData[] _visualDatas;

        public int Count => _visualDatas.Length;

        public GalacticStarVisualData Get(int index)
        {
            return _visualDatas[index];
        }
    }
}
