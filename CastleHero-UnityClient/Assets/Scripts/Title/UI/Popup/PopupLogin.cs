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
    public class PopupLogin : PopupBase
    {
        [SerializeField] private Button[] _platformButtons;

        public UniTask<Response> LoginTask => _completionSource.Task;

        private UniTaskCompletionSource<Response> _completionSource;

        public override UniTask Open(params object[] parameters)
        {
            _completionSource = new UniTaskCompletionSource<Response>();
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
                
                async void OnClick()
                {
                    foreach (var button in _platformButtons)
                    {
                        button.enabled = false;
                    }
                    
                    var response = await service.Login();
                    _completionSource.TrySetResult(response);
                    
                    Close().Forget();
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