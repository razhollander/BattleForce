using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [System.Serializable]
    public class EnvironmentLayoutConfig
    {
        [SerializeField] private string _wallsJson;
        [SerializeField] private string _talentCardsJson;

        public WallConfig[] GetWalls()
        {
            return _wallsJson.FromJson<WallConfig[]>();
        }

        public void SetWallsJson(string wallsJson)
        {
            _wallsJson = wallsJson;
        }

        public TalentCardS2C[] GetTalentCards()
        {
            return _talentCardsJson.FromJson<TalentCardS2C[]>();
        }

        public EnvironmentLayoutConfig(string wallsJson, string talentCardsJson)
        {
            _wallsJson = wallsJson;
            _talentCardsJson = talentCardsJson;
        }
    }
}
