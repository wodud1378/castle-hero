using System;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.View.Sound;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Boot;
using CastleHero.Network.Service.Login;
using CastleHero.View.Lobby.Title.UI.Popup;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Title
{
    public class TitleBehaviour : MonoBehaviour, IBootServiceHandler
    {
        [FormerlySerializedAs("_config")]
        [SerializeField] private BootConfig config;
        [FormerlySerializedAs("_error")]
        [SerializeField] private PopupCommon error;
        [FormerlySerializedAs("_selectPlatform")]
        [SerializeField] private PopupSelectLoginPlatform selectPlatform;
        [FormerlySerializedAs("_policyPopup")]
        [SerializeField] private PopupPolicy policyPopup;
        [FormerlySerializedAs("_setNickname")]
        [SerializeField] private PopupSetNickname setNickname;
        [FormerlySerializedAs("_soundManager")]
        [SerializeField] private SoundManager soundManager;

        private void Awake()
        {
            var factory = ServiceLocator.Get<IBootServiceFactory>();
            var service = factory.Create(config, this);
            service.Start().Forget();
        }

        public UniTask OnError(Result response)
        {
            Debug.LogError(
                $"[result] :{response.error}\n" +
                $"[code] : {response.statusCode}\n" +
                $"[message] : {response.errorMessage}");

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
            selectPlatform.gameObject.SetActive(true);

#if UNITY_EDITOR
            await selectPlatform.Open(Platform.Guest);
#elif UNITY_ANDROID
            await selectPlatform.Open(Platform.PlayStore, Platform.Guest);
#elif UNITY_iOS
            await selectPlatform.Open(Platform.AppStore, Platform.Guest);
#endif
            return await selectPlatform.LoginTask;
        }

        public async UniTask<string> SetNickName()
        {
            setNickname.gameObject.SetActive(true);

            await setNickname.Open();

            return await setNickname.SetNicknameTask;
        }

        public void OnInitDone()
        {
            //ServiceLocator.Get<CastleHero.Common.Sound.ISoundManager>() = null;
            Loading.NextScene = "Main";
        }

        public async UniTask<PolicyAgreement> CheckPolicy()
        {
            const string key = "policy";

            var agree = PlayerPrefs.GetInt(key, 0) == 1;

            PolicyAgreement agreement = default;
            while (!agree)
            {
                policyPopup.gameObject.SetActive(true);

                await policyPopup.Open();
                agreement = await policyPopup.AgreementTask;
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
