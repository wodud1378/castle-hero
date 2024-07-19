using System;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Network;
using RGLabs.Network.Service.Boot;
using RGLabs.Network.Service.Login;
using RGLabs.Title.UI.Popup;
using UnityEngine;

namespace RGLabs.Title
{
    public class TitleBehaviour : MonoBehaviour, IBootServiceHandler
    {
        [SerializeField] private PopupLogin _loginPopup;
        [SerializeField] private PopupPolicy _policyPopup;

        private void Awake()
        {
            var service = new BootService(this);
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

        public async UniTask OnNeedLogin()
        {
            _loginPopup.gameObject.SetActive(true);
            
#if UNITY_EDITOR
            await _loginPopup.Open(Platform.Guest);
#elif UNITY_ANDROID
            await _selectPlatform.Open(Platform.PlayStore, Platform.Guest);
#elif UNITY_iOS
            await _selectPlatform.Open(Platform.AppStore, Platform.Guest);
#endif

            var response = await _loginPopup.LoginTask;
            if (response.result != ResultCode.Success)
            {
                // TODO 로그인 실패 에러 처리.
                return;
            }

            // 신규 유저 약관 동의,
            if (response.row.GetStatusCode() == "201")
                await CheckPolicy();
        }

        private async UniTask CheckPolicy()
        {
            _policyPopup.gameObject.SetActive(true);

            await _policyPopup.Open();
            var agreement = await _policyPopup.AgreementTask;

            PlayerPrefs.SetInt("push", agreement.push ? 1 : 0);
            PlayerPrefs.SetInt("push-night", agreement.nightPush ? 1 : 0);
        }
    }
}