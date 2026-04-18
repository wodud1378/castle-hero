using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Network;
using CastleHero.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.Common.UI.Popup
{
    [PrefabPath("Common/Prefabs/Popups/Popup_Common.prefab")]
    public class PopupCommon : PopupBase
    {
        public struct ButtonParam
        {
            public ButtonAction action;
            public Action onClick;
        }

        public enum ButtonAction
        {
            Confirm,
            Cancel,
        }

        [FormerlySerializedAs("_confirm")]
        [SerializeField] private Button confirm;
        [FormerlySerializedAs("_cancel")]
        [SerializeField] private Button cancel;
        [FormerlySerializedAs("_text")]
        [SerializeField] private TMP_Text text;

        private Action _confirmAction;
        private Action _cancelAction;

        protected override void OnAwake()
        {
            base.OnAwake();

            confirm.gameObject.SetActive(false);
            cancel.gameObject.SetActive(false);

            this.SubscribeButton(confirm, () =>
            {
                _confirmAction?.Invoke();
                CloseAsync().SafeForget();
            });

            this.SubscribeButton(cancel, () =>
            {
                _cancelAction?.Invoke();
                CloseAsync().SafeForget();
            });
        }

        public override UniTask Open(params object[] parameters)
        {
            try
            {
                switch (parameters[0])
                {
                    case string textValue :
                        text.text = textValue;
                        break;
                    case Error error :
                        text.text = error.Text();
                        break;
                }

               if (parameters.Length > 1)
               {
                   if (parameters[1] is IEnumerable<ButtonParam> buttonParams)
                   {
                       foreach (var param in buttonParams)
                       {
                           ApplyButtonParam(param);
                       }
                   }
                   else if (parameters[1] is ButtonParam param)
                   {
                       ApplyButtonParam(param);
                   }
               }

               if (_confirmAction == null && _cancelAction == null)
               {
                   confirm.gameObject.SetActive(true);
               }
               else
               {
                   confirm.gameObject.SetActive(_confirmAction != null);
                   cancel.gameObject.SetActive(_cancelAction != null);
               }

               return UniTask.CompletedTask;
            }
            catch (Exception e)
            {
                return UniTask.FromException(e);
            }
        }

        private void ApplyButtonParam(ButtonParam param)
        {
            switch (param.action)
            {
                case ButtonAction.Confirm:
                    _confirmAction += param.onClick;
                    break;
                case ButtonAction.Cancel:
                    _cancelAction += param.onClick;
                    break;
            }
        }
    }
}
