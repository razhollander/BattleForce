namespace Core.Game.Domains.GamePlay.Shared.C2SModels
{
    public enum PacketTypeC2S : byte
    {
        None = 0,
        MatchPlayerInput = 1,
        MatchRejoinRequest = 2,
        MatchMakingPlayerInput = 3,
        MatchMakingJoinRequest = 4,
        JoinRequest = 5
    }
}