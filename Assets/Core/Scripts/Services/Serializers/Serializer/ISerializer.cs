namespace CoreDomain.Scripts.Services.Serializers.Serializer
{
    public interface ISerializer<TSerialized>
    {
        TSerialized Serialize<TIn>(TIn obj);
        TOut Deserialize<TOut>(TSerialized obj);
        void Populate(TSerialized obj, object target);
    }
}