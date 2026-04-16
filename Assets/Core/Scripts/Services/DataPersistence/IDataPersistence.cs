namespace CoreDomain.Scripts.Services.DataPersistence
{
    public interface IDataPersistence
    {
        void Save<T>(string id, T data);
        void Save<T1, T2>(string id1, T1 data1, string id2, T2 data2);
        void Save<T1, T2, T3>(string id1, T1 data1, string id2, T2 data2, string id3, T3 data3);
        void Save<T1, T2, T3, T4>(string id1, T1 data1, string id2, T2 data2, string id3, T3 data3, string id4, T4 data4);
        void Save<T1, T2, T3, T4, T5>(string id1, T1 data1, string id2, T2 data2, string id3, T3 data3, string id4, T4 data4, string id5, T5 data5);
        T Load<T>(string id, T defaultValue = default);
    }
}