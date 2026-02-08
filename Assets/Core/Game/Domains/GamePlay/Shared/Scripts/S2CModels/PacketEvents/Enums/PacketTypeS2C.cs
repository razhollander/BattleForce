namespace Core.Game.Domains.GamePlay.Shared.C2SModels
{
    public enum PacketTypeS2C : byte
    {
        None = 0,
        MatchFullTick = 1,
        MatchMakingFullTick = 2,
        StartMatch = 3,
        StartStage = 4,
        JoinResponse = 5
    }
}