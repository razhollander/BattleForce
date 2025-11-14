namespace CoreDomain.Scripts.Helpers.Pools
{
    public interface IPool<T> where T : IPoolable
    {
        void InitPool();
        T Spawn();
    }
}