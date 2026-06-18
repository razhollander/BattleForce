namespace Core.Game.Domains.GamePlay.Shared.C2SModels
{
    public enum PacketTypeC2S : byte
    {
        None = 0,
        MatchPlayersInput = 1,
        MatchMakingPlayersInput = 2,
        JoinRequest = 3
    }
}