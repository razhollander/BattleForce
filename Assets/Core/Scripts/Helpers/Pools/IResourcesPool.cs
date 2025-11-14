namespace CoreDomain.Scripts.Helpers.Pools
{
    public interface IResourcesPool<T> : IPool<T> where T : IPoolable
    {
        string AssetPath { get; }
    }
}
