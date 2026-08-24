namespace Core.Game.Domains.GamePlay.Shared.Scripts.Enums
{
    public static class StageTypeExtensions
    {
        // A Bonus Stage is any non-DeathMatch scoring stage (Whac-A-Mole, GatePass, ...). It has no player health,
        // no elimination, a countdown timer, and gems awarded by rank. This is the single definition of the set.
        public static bool IsBonusStage(this StageType stageType)
        {
            return stageType == StageType.WhacAMole || stageType == StageType.GatePass;
        }
    }
}
