using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Network;
using RGLabs.Network.Service.Login;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Title.UI.Popup
{
    public class PopupSelectLoginPlatform : PopupBase
    {
        [SerializeField] private Button[] _platformButtons;

        public UniTask<ILoginService> LoginTask => _completionSource.Task;

        private UniTaskCompletionSource<ILoginService> _completionSource;

        public override UniTask Open(params object[] parameters)
        {
            _completionSource = new UniTaskCompletionSource<ILoginService>();
            foreach (var parameter in parameters)
            {
                if (parameter is not Platform platform)
                    continue;

                var service = GetLoginService(platform);
                if (service == null)
                    continue;

                var current = _platformButtons.FirstOrDefault(x => x.gameObject.name == platform.ToString());
                if (current == null)
                    continue;
                
                current.gameObject.SetActive(true);
                
                void OnClick()
                {
                    foreach (var button in _platformButtons)
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

        private ILoginService GetLoginService(Platform platform)
        {
            return platform switch
            {
                Platform.PlayStore => new GPGSLoginService(),
                Platform.Guest => new GuestLoginService(),
                _ => null
            };
        }
    }
}