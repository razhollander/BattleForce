namespace CoreDomain.Scripts.Services.UpdateService
{
    public interface IGUIUpdatable
    {
        void ManagedOnGUI();
        void ManagedOnDrawGizmos();
    }
}