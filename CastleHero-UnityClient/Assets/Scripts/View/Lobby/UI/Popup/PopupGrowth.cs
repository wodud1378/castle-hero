using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using CastleHero.Common.Sound;
using CastleHero.Common.Pattern;

using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI.Popup
{
    public abstract class PopupGrowth : PopupBase
    {
        [SerializeField] protected UIItemSlot _goldSlot;
        [SerializeField] protected Button _confirm;
        
        public UniTask<UnitGrowth> GrowthTask => _completionSource.Task;

        protected abstract GrowthAction Action { get; }
        
        protected readonly ReactiveProperty<UnitInfo> _unit = new();
        
        private UniTaskCompletionSource<UnitGrowth> _completionSource;
        private UnitGrowth _result;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            ServiceLocator.Get<IUserRepository>().Currency.Gold
                .Subscribe(UpdateGoldSlot)
                .AddTo(this);
            
            ServiceLocator.Get<IUserRepository>().Characters.Units
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(units =>
                {
                    var unit = _unit.Value;
                    if (unit == null)
                        return;

                    var updated = units.FirstOrDefault(x => x.id == unit.id);
                    _unit.Value = updated;
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
            
            var result = await ServiceLocator.Get<INetworkServiceProvider>().Character.Growth(Action, unitId, consume.id, consume.quantity);
            if (!result.IsSuccess)
            {
                ServiceLocator.Get<IPopupManager>().Open<PopupCommon>(result.error);
                return;
            }

            _result = result.data;
            ServiceLocator.Get<ISoundManager>().PlaySfx(ServiceLocator.Get<SoundPath>().levelUp);
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