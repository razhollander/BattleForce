using UnityEngine;

namespace CoreDomain.Scripts.Services.AssetBundleLoaderService
{
    public interface IAssetBundleLoaderService
    {
        bool TryInstantiateAssetFromBundle<T>(string bundlePathName, string assetName, out T asset) where T : Object;
        bool TryLoadGameObjectAssetFromBundle(string bundlePathName, string assetName, out GameObject asset);
        bool TryLoadScriptableObjectAssetFromBundle<T>(string bundlePathName, string assetName, out T asset) where T : ScriptableObject;
        T InstantiateAssetFromBundle<T>(string bundlePathName, string assetName) where T : Object;
        T InstantiateAssetFromBundle<T>(AssetBundle assetBundle, string assetName) where T : Object;
        GameObject LoadGameObjectAssetFromBundle(string bundlePathName, string assetName);
        T LoadScriptableObjectAssetFromBundle<T>(string bundlePathName, string assetName) where T : ScriptableObject;
        AssetBundle LoadAssetBundle(string assetBundlePathName);
        T LoadAssetFromBundle<T>(AssetBundle assetbundle, string assetName) where T : Object;
        void UnloadAssetBundle(AssetBundle assetBundle);
    }
}