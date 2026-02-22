using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [System.Serializable]
    public class EnvironmentLayoutConfig
    {
        [UnityEngine.SerializeField] private string _environmentHalfSizeJson;
        [UnityEngine.SerializeField] private string _wallsJson;
        [UnityEngine.SerializeField] private string _lavaWallsJson;
        [UnityEngine.SerializeField] private string _talentCardsJson;
        [UnityEngine.SerializeField] private string _environmentSpringsJson;
        [UnityEngine.SerializeField] private string _rotatingWheelsJson;

        public EnvironmentLayoutConfig(string wallsJson, string talentCardsJson)
        {
            _wallsJson = wallsJson;
            _talentCardsJson = talentCardsJson;
        }

        public Vector2 GetEnvironmentHalfSize()
        {
            return _environmentHalfSizeJson.FromJson<Vector2>();
        }

        public WallConfig[] GetWalls()
        {
            return _wallsJson.FromJson<WallConfig[]>();
        }

        public WallConfig[] GetLavaWalls()
        {
            return _lavaWallsJson.FromJson<WallConfig[]>();
        }

        public void SetWallsJson(string wallsJson)
        {
            _wallsJson = wallsJson;
        }

        public void SetLavaWallsJson(string lavaWallsJson)
        {
            _lavaWallsJson = lavaWallsJson;
        }

        public void SetEnvironmentSpringsJson(string environmentSpringsJson)
        {
            _environmentSpringsJson = environmentSpringsJson;
        }

        public void SetRotatingWheelsJson(string rotatingWheelsJson)
        {
            _rotatingWheelsJson = rotatingWheelsJson;
        }

        public TalentCardS2C[] GetTalentCards()
        {
            return _talentCardsJson.FromJson<TalentCardS2C[]>();
        }

        public EnvironmentSpringS2C[] GetEnvironmentSprings()
        {
            return _environmentSpringsJson.FromJson<EnvironmentSpringS2C[]>();
        }

        public EnvironmentRotatingWheelConfig[] GetRotatingWheels()
        {
            return _rotatingWheelsJson.FromJson<EnvironmentRotatingWheelConfig[]>();
        }
    }
}
