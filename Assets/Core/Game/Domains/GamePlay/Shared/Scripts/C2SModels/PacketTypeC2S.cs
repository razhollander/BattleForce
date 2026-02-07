namespace Core.Game.Domains.GamePlay.Shared.C2SModels
{
    public enum PacketTypeC2S : byte
    {
        None = 0,
        MatchPlayerInput = 1,
        MatchMakingPlayerInput = 2,
        JoinRequest = 3
    }
}