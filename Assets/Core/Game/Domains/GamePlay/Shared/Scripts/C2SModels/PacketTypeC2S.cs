namespace Core.Game.Domains.GamePlay.Shared.C2SModels
{
    public enum PacketTypeC2S : byte
    {
        None = 0,
        PlayerInput = 1,
        JoinRequest = 2
    }
}