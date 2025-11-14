namespace CoreDomain.Scripts.Services.CommandFactory
{
    public interface ICommandFactory
    {
        TCommand CreateCommandVoid<TCommand>() where TCommand : ICommandVoid, new();
        TCommand CreateCommandWithResult<TCommand, TReturn>() where TCommand : ICommandWithResult<TReturn>, new();
        TCommand CreateCommandAsync<TCommand>() where TCommand : ICommandAsync, new();
        TCommand CreateCommandAsyncWithResult<TCommand, TReturn>() where TCommand : ICommandAsyncWithResult<TReturn>, new();
    }
}