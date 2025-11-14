using System;
using CoreDomain.Scripts.Services.Logger.Base;
using CoreDomain.Scripts.Services.Serializers.Serializer;
using CoreDomain.Scripts.Utils;
using UnityEngine;

namespace CoreDomain.Scripts.Services.DataPersistence
{
    public class PlayerPrefsDataPersistence : IDataPersistence
    {
        private readonly ISerializerService _serializer;

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
                if (!PlayerPrefs.HasKey(id))
                    return defaultValue;

                var encrypted = PlayerPrefs.GetString(id);
                var json = EncryptionUtils.Decrypt(encrypted);
                return _serializer.DeserializeJson<T>(json);
            }
            catch (Exception e)
            {
                LogService.LogError($"Tried to load {id}, but exception was thrown: {e}");
                throw;
            }
        }
    }
}