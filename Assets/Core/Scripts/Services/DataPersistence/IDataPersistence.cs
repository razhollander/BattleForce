namespace CoreDomain.Scripts.Services.DataPersistence
{
    public interface IDataPersistence
    {
        void Save<T>(string id, T data);
        T Load<T>(string id, T defaultValue = default);
    }
}