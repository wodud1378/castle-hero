using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Inventory;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UniRx;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Refine.prefab")]
    public class PopupRefine : PopupLeftToRight<EquipItem, UIEquipmentSlot>, ISelect<bool>
    {
        private readonly ReactiveProperty<EquipItem> _equipItem = new();
        private readonly ReactiveProperty<ItemEntity> _stoneItem = new();

        public UniTask<bool> SelectTask => _ctSource.Task;
        
        private UniTaskCompletionSource<bool> _ctSource;
        private bool _closeAfterSelect;

        public void BeginSelect(bool closeAfterSelect = true)
        {
            _ctSource = new UniTaskCompletionSource<bool>();
            _closeAfterSelect = closeAfterSelect;
        }
        
        protected override void OnAwake()
        {
            base.OnAwake();

            Observable.Merge(
                    _equipItem.Select(_ => UniRx.Unit.Default),
                    _stoneItem.Select(_ => UniRx.Unit.Default))
                .Subscribe(_ => OnDataChanged())
                .AddTo(this);
        }

        protected override void HandleParameters(params object[] parameters)
        {
            if (parameters[0] is EquipItem equipItem)
                _equipItem.Value = equipItem;

            if (parameters[1] is ItemEntity entity)
                _stoneItem.Value = entity;
        }

        protected override void OnClose()
        {
            base.OnClose();
            
            if (_ctSource != null)
            {
                _ctSource.TrySetResult(false);
                _ctSource = null;
            }
        }

        private void OnDataChanged()
        {
            var l = _equipItem.Value;
            var stone = _stoneItem.Value;
            if (l == null || !stone.IsValid || !stone.TryGetElementalOption(out var option))
                return;

            var r = new EquipItem
            {
                element = new EquipItem.Element
                {
                    type = (int)option.type,
                    lv = option.lv
                },
                Guid = l.Guid,
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                character = l.character,
                slot = l.slot,
                main = l.main,
                sub = l.sub
            };

            left.Value = l;
            right.Value = r;
        }

        protected override UniTask InitSlot(EquipItem data, UIEquipmentSlot slot) => slot.Init(data);

        protected override async void OnSubmit()
        {
            if (_ctSource != null)
            {
                _ctSource.TrySetResult(true);
                _ctSource = null;
            }
            else
            {
                var result = await NetworkService.Inventory.Refine(_equipItem.Value.Guid, _stoneItem.Value.Id);
                if (!result.IsSuccess)
                {
                    Context.popups.Open<PopupCommon>(result.error);
                    return;
                }
            }

            Close();
        }
    }
}