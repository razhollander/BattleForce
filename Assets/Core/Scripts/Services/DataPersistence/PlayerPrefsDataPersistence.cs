using System;
using System.Collections.Generic;
using CoreDomain.Scripts.Services.DataPersistence;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.Serializers.Serializer;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace Core.Scripts.Services.DataPersistence
{
    public class PlayerPrefsDataPersistence : IDataPersistence
    {
        private readonly ISerializerService _serializer;
        private readonly Dictionary<string, string> _cachedIdToJson = new();

        public PlayerPrefsDataPersistence(ISerializerService serializer)
        {
            _serializer = serializer;
        }

        private void SaveInternal<T>(string id, T data)
        {
            try
            {
                var json = _serializer.SerializeJson(data);
                var encrypted = EncryptionUtils.Encrypt(json);

                PlayerPrefs.SetString(id, encrypted);

                // update cache
                _cachedIdToJson[id] = json;
            }
            catch (Exception e)
            {
                LogService.LogError($"Tried to save {id}, but exception was thrown: {e}");
            }
        }
        
        public void Save<T>(string id, T data)
        {
            SaveInternal(id, data);
            PlayerPrefs.Save();
        }
        
        public void Save<T1, T2>(string id1, T1 data1, string id2, T2 data2)
        {
            SaveInternal(id1, data1);
            SaveInternal(id2, data2);
            PlayerPrefs.Save();
        }
        
        public void Save<T1, T2, T3>(string id1, T1 data1, string id2, T2 data2, string id3, T3 data3)
        {
            SaveInternal(id1, data1);
            SaveInternal(id2, data2);
            SaveInternal(id3, data3);
            PlayerPrefs.Save();
        }
        
        public void Save<T1, T2, T3, T4>(string id1, T1 data1, string id2, T2 data2, string id3, T3 data3, string id4, T4 data4)
        {
            SaveInternal(id1, data1);
            SaveInternal(id2, data2);
            SaveInternal(id3, data3);
            SaveInternal(id4, data4);
            PlayerPrefs.Save();
        }
        
        public void Save<T1, T2, T3, T4, T5>(string id1, T1 data1, string id2, T2 data2, string id3, T3 data3, string id4, T4 data4, string id5, T5 data5)
        {
            Save(id1, data1);
            Save(id2, data2);
            Save(id3, data3);
            Save(id4, data4);
            Save(id5, data5);
            PlayerPrefs.Save();
        }

        public T Load<T>(string id, T defaultValue = default)
        {
            try
            {
                if (_cachedIdToJson.TryGetValue(id, out var cachedJson))
                {
                    return _serializer.DeserializeJson<T>(cachedJson);
                }

                if (!PlayerPrefs.HasKey(id))
                    return defaultValue;

                var json = CacheIdAsJson(id);
                return _serializer.DeserializeJson<T>(json);
            }
            catch (Exception e)
            {
                LogService.LogError($"Tried to load {id}, but exception was thrown: {e}");
                throw;
            }
        }

        private string CacheIdAsJson(string id)
        {
            var encrypted = PlayerPrefs.GetString(id);
            var json = EncryptionUtils.Decrypt(encrypted);
            _cachedIdToJson[id] = json;
            return json;
        }
    }
}