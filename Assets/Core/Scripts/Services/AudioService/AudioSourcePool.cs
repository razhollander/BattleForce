using CoreDomain.Scripts.Helpers.Pools;
using Zenject;

namespace Core.Scripts.Services.AudioService
{
    public class AudioSourcePool : PrefabsPool<AudioSourcePoolable>
    {
        public AudioSourcePool(PoolData poolData, DiContainer diContainer, AudioSourcePoolable prefab) : base(poolData, diContainer, prefab)
        {
        }

        protected override string ParentGameObjectName => "AudioSourcesParent";
    }
}