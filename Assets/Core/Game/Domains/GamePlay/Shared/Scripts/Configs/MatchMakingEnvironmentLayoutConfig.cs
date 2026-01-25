using System.Collections.Generic;
using System.Numerics;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [System.Serializable]
    public class MatchMakingEnvironmentLayoutConfig
    {
        [UnityEngine.SerializeField] private string _environmentHalfSizeJson;
        public float TeamFloorsRadius=10;
        public int TeamFloorsPrecision=10;
        public float WallsWidth=2;
        
        public Vector2 GetEnvironmentHalfSize()
        {
            return _environmentHalfSizeJson.FromJson<Vector2>();
        }

        public WallConfig[] GetWalls()
        {
            return DonutQuadrantWalls.GenerateWrapAroundWallJson(TeamFloorsRadius, WallsWidth, TeamFloorsPrecision);
        }
    }
}