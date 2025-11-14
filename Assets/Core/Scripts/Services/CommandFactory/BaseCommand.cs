using Zenject;

namespace CoreDomain.Scripts.Services.CommandFactory
{
    public abstract class BaseCommand : IBaseCommand
    {
        protected DiContainer _diContainer;

        public void SetObjectResolver(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public abstract void ResolveDependencies();
    }
}