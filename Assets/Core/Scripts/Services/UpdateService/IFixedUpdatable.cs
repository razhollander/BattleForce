namespace CoreDomain.Scripts.Services.UpdateService
{
    public interface IClientNetworkTickProcessor
    {
    }

    public interface IFixedUpdatable : IClientNetworkTickProcessor
    {
        public void ManagedFixedUpdate();
    }
}