using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Data.User;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RGLabs.InGame.UI
{
    public class UICharacterSlot : UIItemSlot, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<UICharacterSlot> OnBeginDrag;
        public event Action OnDuringDrag;
        public event Action<UICharacterSlot> OnEndDrag;

        private const float PressThreshold = 0.2f;

        public Character Data { get; private set; }
        public UnitEntity Entity { get; private set; }

        private bool _onPress;
        private bool _isPressed;

        private float _pressTime = 0f;

        public async UniTask InitAsync(Character data, UnitDB db)
        {
            Data = data;

            if (!db.TryFind(data.id, out var entity))
                return;

            Entity = entity;

            await base.InitAsync(entity.icon);
        }

        public void OnPointerDown(PointerEventData eventData) => _onPress = true;
        
        public void OnPointerUp(PointerEventData eventData)
        {
            _onPress = false;
            _isPressed = false;
            _pressTime = 0f;
            OnEndDrag?.Invoke(this);
        }

        private void Update()
        {
            if (!_onPress)
                return;

            if (_isPressed)
            {
                OnDuringDrag?.Invoke();
                return;
            }

            _pressTime += Time.deltaTime;
            if (_pressTime < PressThreshold)
                return;
            
            OnBeginDrag?.Invoke(this);
            _isPressed = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            OnBeginDrag = null;
            OnDuringDrag = null;
            OnEndDrag = null;
        }
    }
}