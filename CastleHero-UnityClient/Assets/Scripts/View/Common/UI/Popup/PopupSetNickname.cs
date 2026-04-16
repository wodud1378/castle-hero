using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.Common.UI.Popup
{
    public class PopupSetNickname : PopupBase
    {
        [FormerlySerializedAs("_input")]
        [SerializeField] private TMP_InputField input;
        [FormerlySerializedAs("_submit")]
        [SerializeField] private Button submit;
        [FormerlySerializedAs("_warning")]
        [SerializeField] private GameObject warning;

        private UniTaskCompletionSource<string> _completionSource;

        public UniTask<string> SetNicknameTask => _completionSource.Task;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(submit, OnSubmit);

            warning.SetActive(false);
            submit.interactable = false;

            input.onValueChanged
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
            var text = input.text;
            bool isValid =
                text.Length is <= 12 and >= 2 &&
                !text.StartsWith(" ") &&
                !text.EndsWith(" ") &&
                Regex.IsMatch(text, @"^[a-zA-Z0-9가-힣\s]*$");

            warning.SetActive(false);
            submit.interactable = isValid;
        }

        private void OnSubmit()
        {
            _completionSource.TrySetResult(input.text);

            CloseAsync().Forget();
        }
    }
}
