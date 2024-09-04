using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    public abstract class PopupGrowth : PopupBase
    {
        [SerializeField] protected UIItemSlot _goldSlot;
        [SerializeField] protected Button _confirm;
        
        public UniTask<UnitGrowth> GrowthTask => _completionSource.Task;

        protected abstract CharacterService.GrowthAction Action { get; }
        
        protected readonly ReactiveProperty<UnitInfo> _unit = new();
        
        private UniTaskCompletionSource<UnitGrowth> _completionSource;
        private UnitGrowth _result;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            Storage.userRepository.currency.gold
                .Subscribe(UpdateGoldSlot)
                .AddTo(this);
            
            Storage.userRepository.characters.units
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(units =>
                {
                    var unit = _unit.Value;
                    if (unit == null)
                        return;

                    var updated = units.FirstOrDefault(x => x.id == unit.id);
                    _unit.Value = updated;
                    
                    Storage.inGameRepository.UpdateIfInField(updated);
                })
                .AddTo(this);
            
            _unit
                .Subscribe(OnUnitChangedInternal)
                .AddTo(this);
            
            this.SubscribeButton(_confirm, () => Confirm().Forget());
        }

        public override UniTask Open(params object[] parameters)
        {
            _completionSource = new();

            if (parameters.Length > 0 && parameters[0] is UnitInfo unit)
                _unit.Value = unit;
            
            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            base.OnClose();
            
            _completionSource.TrySetResult(_result);
        }

        private void OnUnitChangedInternal(UnitInfo unit)
        {
            if (unit == null)
                return;
            
            OnUnitChanged(unit);
        }

        protected abstract void OnUnitChanged(UnitInfo unit);

        protected abstract (int id, int quantity) ConsumeItem();

        private async UniTaskVoid Confirm()
        {
            var unitId = _unit.Value.id;
            var consume = ConsumeItem();
            if (consume.quantity == 0)
                return;
            
            var result = await NetworkService.Character.Growth(Action, unitId, consume.id, consume.quantity);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
                return;
            }

            _result = result.data;
            Context.sounds.PlaySfx(Storage.soundPath.levelUp);
        }
        
        private void UpdateGoldSlot(int gold)
        {
            _goldSlot.Init(new Item
            {
                ItemId = Constants.GoldId,
                Quantity = gold
            });
        }
    }
}