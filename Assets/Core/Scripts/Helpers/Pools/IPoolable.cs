using System;

namespace CoreDomain.Scripts.Helpers.Pools
{
    public interface IPoolable
    {
        void OnCreated();
        Action Despawn { set; }
        void OnSpawned();
        void OnDespawned();
    }
}