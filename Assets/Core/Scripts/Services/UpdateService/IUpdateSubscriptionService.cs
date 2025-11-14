namespace CoreDomain.Scripts.Services.UpdateService
{
    public interface IUpdateSubscriptionService
    {
        void RegisterUpdatable(IUpdatable updatable);
        void UnregisterUpdatable(IUpdatable updatable);
        void RegisterLateUpdatable(ILateUpdatable updatable);
        void UnregisterLateUpdatable(ILateUpdatable updatable);
        void RegisterFixedUpdatable(IFixedUpdatable updatable);
        void UnregisterFixedUpdatable(IFixedUpdatable updatable);
    }
}