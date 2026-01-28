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

        public void Save<T>(string id, T data)
        {
            try
            {
                var json = _serializer.SerializeJson(data);
                var encrypted = EncryptionUtils.Encrypt(json);

                PlayerPrefs.SetString(id, encrypted);
                PlayerPrefs.Save();

                // update cache
                _cachedIdToJson[id] = json;
            }
            catch (Exception e)
            {
                LogService.LogError($"Tried to save {id}, but exception was thrown: {e}");
            }
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