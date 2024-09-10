using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Sound;
using RGLabs.Common.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Service.Boot;
using RGLabs.Network.Service.Login;
using RGLabs.Title.UI.Popup;
using UnityEngine;
using JsonConvert = BackEnd.BackndNewtonsoft.Json.JsonConvert;

namespace RGLabs.Title
{
    public struct PolicyAgreement
    {
        public bool terms;
        public bool privacy;
        public bool share;
        public bool push;
        public bool nightPush;

        [JsonIgnore] public bool updated;
    }

    public class TitleBehaviour : MonoBehaviour, IBootServiceHandler
    {
        [SerializeField] private BootConfig _config;
        [SerializeField] private PopupCommon _error;
        [SerializeField] private PopupSelectLoginPlatform _selectPlatform;
        [SerializeField] private PopupPolicy _policyPopup;
        [SerializeField] private PopupSetNickname _setNickname;
        [SerializeField] private SoundManager _soundManager;

        private void Awake()
        {
            var service = new BootService(_config, this);
            service.Start().Forget();

            //Context.sounds = _soundManager;
        }

        public UniTask OnError(Result response)
        {
            Debug.LogError(
                $"[result] :{response.error}\n" +
                $"[code] : {response.statusCode}" +
                $"[raw] : {response.raw.GetFlattenJSON().ToJson()}");

            return UniTask.CompletedTask;
        }

        public UniTask OnMaintenance()
        {
            throw new NotImplementedException();
        }

        public UniTask OnForceUpdate()
        {
            throw new NotImplementedException();
        }

        public async UniTask<ILoginService> ProvideLoginService()
        {
            _selectPlatform.gameObject.SetActive(true);

#if UNITY_EDITOR
            await _selectPlatform.Open(Platform.Guest);
#elif UNITY_ANDROID
            await _selectPlatform.Open(Platform.PlayStore, Platform.Guest);
#elif UNITY_iOS
            await _selectPlatform.Open(Platform.AppStore, Platform.Guest);
#endif
            return await _selectPlatform.LoginTask;
        }

        public async UniTask<string> SetNickName()
        {
            _setNickname.gameObject.SetActive(true);

            await _setNickname.Open();

            return await _setNickname.SetNicknameTask;
        }

        public void OnInitDone()
        {
            //Context.sounds = null;
            Loading.NextScene = "Main";
        }

        public async UniTask<PolicyAgreement> CheckPolicy()
        {
            const string key = "policy";
         
            var agree = PlayerPrefs.GetInt(key, 0) == 1;
            
            PolicyAgreement agreement = default;
            while (!agree)
            {
                _policyPopup.gameObject.SetActive(true);

                await _policyPopup.Open();
                agreement = await _policyPopup.AgreementTask;
                agree = agreement is { terms: true, privacy: true };
            }
            
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.SetInt("push", agreement.push ? 1 : 0);
            PlayerPrefs.SetInt("push-night", agreement.nightPush ? 1 : 0);
            PlayerPrefs.Save();
            
            return agreement;
        }
    }
}