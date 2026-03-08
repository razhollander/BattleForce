using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [System.Serializable]
    public class EnvironmentLayoutConfig
    {
        [TextArea(1, 5)] [SerializeField] private string _environmentHalfSizeJson;
        [TextArea(1, 5)] [SerializeField] private string _wallsJson;
        [TextArea(1, 5)] [SerializeField] private string _lavaWallsJson;
        [TextArea(1, 5)] [SerializeField] private string _talentCardsJson;
        [TextArea(1, 5)] [SerializeField] private string _environmentSpringsJson;
        [TextArea(1, 5)] [SerializeField] private string _teleportGatesJson;
        [TextArea(1, 5)] [SerializeField] private string _rotatingWheelsJson;
        [TextArea(1, 5)] [SerializeField] private string _fieldBarriersJson;

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

        public void SetTeleportGatesJson(string teleportGatesJson)
        {
            _teleportGatesJson = teleportGatesJson;
        }

        public void SetRotatingWheelsJson(string rotatingWheelsJson)
        {
            _rotatingWheelsJson = rotatingWheelsJson;
        }

        public void SetFieldBarriersJson(string fieldBarriersJson)
        {
            _fieldBarriersJson = fieldBarriersJson;
        }

        public TalentCardConfig[] GetTalentCards()
        {
            return _talentCardsJson.FromJson<TalentCardConfig[]>();
        }

        public EnvironmentSpringConfig[] GetEnvironmentSprings()
        {
            return _environmentSpringsJson.FromJson<EnvironmentSpringConfig[]>();
        }

        public EnvironmentTeleportGatePairConfig[] GetTeleportGates()
        {
            return _teleportGatesJson.FromJson<EnvironmentTeleportGatePairConfig[]>();
        }

        public EnvironmentRotatingWheelConfig[] GetRotatingWheels()
        {
            return _rotatingWheelsJson.FromJson<EnvironmentRotatingWheelConfig[]>();
        }

        public EnvironmentFieldBarrierConfig[] GetFieldBarriers()
        {
            return _fieldBarriersJson.FromJson<EnvironmentFieldBarrierConfig[]>();
        }
    }
}
