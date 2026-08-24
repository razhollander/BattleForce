namespace Core.Game.Domains.GamePlay.Shared.Scripts.Enums
{
    // Tells what the caught enemy id refers to, since a fishing rod can hook either an enemy player or a mole.
    public enum FishingRodCaughtEnemyType : byte
    {
        None = 0,
        Player = 1,
        Mole = 2,
    }
}
