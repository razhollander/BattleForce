namespace Core.Scripts.Services.ApplicationSubscriptionService
{
    public interface IApplicationObserver
    {
        void OnApplicationQuit();
        void OnApplicationFocus(bool hasFocus);
    }
}