namespace CoreDomain.Scripts.Services.CommandFactory
{
    public interface ICommandWithResult<TReturn> : IBaseCommand
    {
        TReturn Execute();
    }
}