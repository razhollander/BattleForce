using Zenject;

namespace CoreDomain.Scripts.Services.CommandFactory
{
    public interface IBaseCommand
    {
        void SetObjectResolver(DiContainer diContainer);
        void ResolveDependencies();
    }
}