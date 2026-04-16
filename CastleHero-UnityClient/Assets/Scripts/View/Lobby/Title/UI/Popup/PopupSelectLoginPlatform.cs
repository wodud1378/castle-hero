using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service.Login;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Title.UI.Popup
{
    public class PopupSelectLoginPlatform : PopupBase
    {
        [FormerlySerializedAs("_platformButtons")]
        [SerializeField] private Button[] platformButtons;

        public UniTask<ILoginService> LoginTask => _completionSource.Task;

        private UniTaskCompletionSource<ILoginService> _completionSource;
        private ILoginServiceFactory _loginFactory;

        protected override void OnAwake()
        {
            base.OnAwake();
            _loginFactory = ServiceLocator.Get<ILoginServiceFactory>();
        }

        public override UniTask Open(params object[] parameters)
        {
            _completionSource = new UniTaskCompletionSource<ILoginService>();
            foreach (var parameter in parameters)
            {
                if (parameter is not Platform platform)
                    continue;

                var service = _loginFactory.Create(platform);
                if (service == null)
                    continue;

                var current = platformButtons.FirstOrDefault(x => x.gameObject.name == platform.ToString());
                if (current == null)
                    continue;

                current.gameObject.SetActive(true);

                void OnClick()
                {
                    foreach (var button in platformButtons)
                    {
                        button.enabled = false;
                    }

                    _completionSource.TrySetResult(service);

                    CloseAsync().Forget();
                }

                this.SubscribeButton(current, OnClick);
            }

            return UniTask.CompletedTask;
        }
    }
}
