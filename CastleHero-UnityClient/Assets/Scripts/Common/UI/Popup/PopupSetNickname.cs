using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using RGLabs.Network;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI.Popup
{
    public class PopupSetNickname : PopupBase
    {
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _submit;
        [SerializeField] private GameObject _warning;

        private UniTaskCompletionSource<string> _completionSource;

        public UniTask<string> SetNicknameTask => _completionSource.Task;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeButton(_submit, OnSubmit);
            
            _warning.SetActive(false);
            _submit.interactable = false;
            
            _input.onValueChanged
                .AsObservable()
                .Subscribe(_=> Validate());
        }

        public override UniTask Open()
        {
            _completionSource = new();

            return UniTask.CompletedTask;
        }

        private void Validate()
        {
            var text = _input.text;
            bool isValid =
                text.Length is <= 12 and >= 2 &&
                !text.StartsWith(" ") &&
                !text.EndsWith(" ") &&
                Regex.IsMatch(text, @"^[a-zA-Z0-9\s]*$");
            
            _warning.SetActive(false);
            _submit.interactable = isValid;
        }
        
        private void OnSubmit()
        {
            _completionSource.TrySetResult(_input.text);
            
            CloseAsync().Forget();
        }
    }
}