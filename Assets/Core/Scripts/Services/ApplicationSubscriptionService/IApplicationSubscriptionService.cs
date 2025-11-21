namespace Core.Scripts.Services.ApplicationSubscriptionService
{
    public interface IApplicationSubscriptionService
    {
        void RegisterObserver(IApplicationObserver applicationObserver);
        void UnregisterObserver(IApplicationObserver applicationObserver);
    }
}