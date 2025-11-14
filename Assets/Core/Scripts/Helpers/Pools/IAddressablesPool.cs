namespace CoreDomain.Scripts.Helpers.Pools
{
    public interface IAddressablesPool<T> : IPoolAsync<T> where T : IPoolable
    {
        string AssetAdress { get; }
    }
}
