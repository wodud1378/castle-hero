using System;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Network;
using RGLabs.Network.Service.Boot;
using RGLabs.Network.Service.Login;
using RGLabs.Title.UI.Popup;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLabs.Title
{
    public struct PolicyAgreement
    {
        public bool terms;
        public bool privacy;
        public bool push;
        public bool nightPush;
    }

    public class TitleBehaviour : MonoBehaviour, IBootServiceHandler
    {
        [SerializeField] private BootConfig _config;
        [SerializeField] private PopupCommon _error;
        [SerializeField] private PopupSelectLoginPlatform _selectPlatform;
        [SerializeField] private PopupPolicy _policyPopup;
        [SerializeField] private PopupSetNickname _setNickname;

        private void Awake()
        {
            var service = new BootService(_config, this);
            service.Start().Forget();
        }

        public UniTask OnError(Response response)
        {
            Debug.LogError(
                $"[result] :{response.result}\n" +
                $"[code] : {response.statusCode}" +
            $"[raw] : {response.rawData.ToJson()}");
            
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
            Loading.NextScene = "Main";
        }

        public async UniTask<PolicyAgreement> CheckPolicy()
        {
            _policyPopup.gameObject.SetActive(true);

            await _policyPopup.Open();
            var agreement = await _policyPopup.AgreementTask;

            PlayerPrefs.SetInt("push", agreement.push ? 1 : 0);
            PlayerPrefs.SetInt("push-night", agreement.nightPush ? 1 : 0);

            return agreement;
        }
    }
}