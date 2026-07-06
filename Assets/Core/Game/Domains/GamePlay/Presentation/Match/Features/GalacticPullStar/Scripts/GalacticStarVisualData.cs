using System;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts
{
    [Serializable]
    public class GalacticStarVisualData
    {
        [SerializeField] private Sprite _planetSprite;
        [SerializeField] private Material _gravityForceMaterial;

        public Sprite PlanetSprite => _planetSprite;
        public Material GravityForceMaterial => _gravityForceMaterial;
    }
}
