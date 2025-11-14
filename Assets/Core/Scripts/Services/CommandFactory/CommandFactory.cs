using Zenject;

namespace CoreDomain.Scripts.Services.CommandFactory
{
    public class CommandFactory : ICommandFactory
    {
        private readonly DiContainer _diContainer;

        public CommandFactory(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }
        
        public TCommand CreateCommandVoid<TCommand>() where TCommand : ICommandVoid, new()
        {
            var command = new TCommand();
            //_diContainer.Inject(command);
            command.SetObjectResolver(_diContainer);
            command.ResolveDependencies();
            return command;
        }
        
        public TCommand CreateCommandWithResult<TCommand,TReturn>() where TCommand : ICommandWithResult<TReturn>, new()
        {
            var command = new TCommand();
            command.SetObjectResolver(_diContainer);
            command.ResolveDependencies();
            return command;
        }
        
        public TCommand CreateCommandAsync<TCommand>() where TCommand : ICommandAsync, new()
        {
            var command = new TCommand();
            _diContainer.Inject(command);
            command.SetObjectResolver(_diContainer);
            command.ResolveDependencies();
            return command;
        }
        
        public TCommand CreateCommandAsyncWithResult<TCommand,TReturn>() where TCommand : ICommandAsyncWithResult<TReturn>, new()
        {
            var command = new TCommand();
            command.SetObjectResolver(_diContainer);
            command.ResolveDependencies();
            return command;
        }
    }
}