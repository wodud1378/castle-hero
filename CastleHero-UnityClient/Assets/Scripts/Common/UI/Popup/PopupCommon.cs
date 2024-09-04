using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI.Popup
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

        [SerializeField] private Button _confirm;
        [SerializeField] private Button _cancel;
        [SerializeField] private TMP_Text _text;

        private Action _confirmAction;
        private Action _cancelAction;

        protected override void OnAwake()
        {
            base.OnAwake();
            
            _confirm.gameObject.SetActive(false);
            _cancel.gameObject.SetActive(false);

            this.SubscribeButton(_confirm, () =>
            {
                _confirmAction?.Invoke();
                CloseAsync().Forget();
            });
            
            this.SubscribeButton(_cancel, () =>
            {
                _cancelAction?.Invoke();
                CloseAsync().Forget();
            });
        }

        public override UniTask Open(params object[] parameters)
        {
            try
            {
                switch (parameters[0])
                {
                    case string text :
                        _text.text = text;
                        break;
                    case Error error :
                        _text.text = error.Text();
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
                   _confirm.gameObject.SetActive(true);
               }
               else
               {
                   _confirm.gameObject.SetActive(_confirmAction != null);
                   _cancel.gameObject.SetActive(_cancelAction != null);
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