using CastleHero.Common.Secure;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace CastleHero.Network.Impl.Local
{
    /// <summary>
    /// PlayerPrefs (EncryptStore) 에 UserDataDto 를 직렬화하여 보관.
    /// 첫 진입 시 비어있으며, CreateUserData 호출 후 영속화된다.
    /// IItem 다형성을 위해 Newtonsoft.Json + TypeNameHandling.Auto 를 사용.
    /// </summary>
    public class LocalUserDataStore
    {
        private const string PrefsKey = "castle_local_user_data";

        private static readonly JsonSerializerSettings Settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        private UserDataDto _cache;

        public bool HasData => _cache != null || !string.IsNullOrEmpty(EncryptStore.GetString(PrefsKey));

        public UserDataDto Load()
        {
            if (_cache != null)
                return _cache;

            var raw = EncryptStore.GetString(PrefsKey);
            if (string.IsNullOrEmpty(raw))
                return null;

            _cache = JsonConvert.DeserializeObject<UserDataDto>(raw, Settings);
            return _cache;
        }

        public void Save(UserDataDto data)
        {
            _cache = data;

            var json = JsonConvert.SerializeObject(data, Settings);
            EncryptStore.SetString(PrefsKey, json);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            _cache = null;
            EncryptStore.SetString(PrefsKey, string.Empty);
            PlayerPrefs.Save();
        }
    }
}
