namespace CoreDomain.Scripts.Helpers.Pools
{
    public interface IAssetFromBundlePool<T> : IPool<T> where T : IPoolable
    {
        string AssetName { get; }
    }
}