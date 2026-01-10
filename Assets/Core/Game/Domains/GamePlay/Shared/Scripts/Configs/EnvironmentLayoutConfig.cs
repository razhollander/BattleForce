namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [System.Serializable]
    public class EnvironmentLayoutConfig
    {
        public WallConfig[] Walls;
        public TalentCard[] TalentCards;

        public EnvironmentLayoutConfig(WallConfig[] walls, TalentCard[] talentCards)
        {
            Walls = walls;
            TalentCards = talentCards;
        }
    }
}
