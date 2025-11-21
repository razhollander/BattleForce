namespace Core.Game.Domains.GamePlay.Shared.NetworkManager
{
    public interface INetworkTickProcessor
    {
        void StartTick(int ticksPerSecond);
    }
}