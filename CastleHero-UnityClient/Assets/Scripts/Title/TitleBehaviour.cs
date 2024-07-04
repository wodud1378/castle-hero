using System;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
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
        [FormerlySerializedAs("_loginPopup")] [SerializeField] private PopupSelectLoginPlatform selectLoginPlatformPopupSelect;
        [SerializeField] private PopupPolicy _policyPopup;

        private void Awake()
        {
            var service = new BootService(_config, this);
            service.Start().Forget();

            Backend.ErrorHandler.InitializePoll(true);
        }

        private void Update()
        {
            Backend.AsyncPoll();
            Backend.ErrorHandler.Poll();
        }

        public UniTask OnError(Response response)
        {
            throw new NotImplementedException();
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
            selectLoginPlatformPopupSelect.gameObject.SetActive(true);
            
#if UNITY_EDITOR
            await selectLoginPlatformPopupSelect.Open(Platform.Guest);
#elif UNITY_ANDROID
            await _loginPopup.Open(Platform.PlayStore, Platform.Guest);
#elif UNITY_iOS
            await _loginPopup.Open(Platform.AppStore, Platform.Guest);
#endif
            return await selectLoginPlatformPopupSelect.LoginTask;
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