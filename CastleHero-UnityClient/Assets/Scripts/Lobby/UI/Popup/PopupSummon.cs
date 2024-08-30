using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

using Probability = System.Collections.Generic.Dictionary<int, (string name, float weight)>;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Summon/PopupSummon.prefab")]
    public class PopupSummon : PopupBase
    {
        private static readonly int OnceTrigger = Animator.StringToHash("Action_1");
        private static readonly int TenthTrigger = Animator.StringToHash("Action_10");

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _desc;
        [SerializeField] private Button _info;
        [SerializeField] private Button _prev;
        [SerializeField] private Button _next;
        [SerializeField] private Button _x1;
        [SerializeField] private UIItemSlot _itemForX1;
        [SerializeField] private Button _x10;
        [SerializeField] private UIItemSlot _itemForX10;

        private readonly Dictionary<int, Probability> _probabilityCache = new();
        private readonly ReactiveProperty<SummonEntity> _entity = new();

        private Summon _result;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_info, () => OpenInfo().Forget());
            this.SubscribeButton(_prev, OnPrev);
            this.SubscribeButton(_next, OnNext);
        }
        
        public override UniTask Open() => Open(Storage.db.summons[0]);

        public override UniTask Open(params object[] parameters)
        {
            if(parameters[0] is not SummonEntity entity)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            _entity
                .Subscribe(OnEntityChanged)
                .AddTo(this);

            _entity.Value = entity;
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid OpenInfo()
        {
            var data = GetOrCreateProbability(_entity.Value.groupId);
            var infoString = BuildInfoString(data);

            var popup = await Context.popups
                .OpenAsync<PopupCommon>(infoString);

            var buttonRect = (_info.transform as RectTransform)!;
            var popupRect = (popup.transform as RectTransform)!;

            popupRect.Attach(buttonRect, new Vector2(0f, 1f));
        }

        private void AddDataIndex(int value)
        {
            var db = Storage.db.summons;
            if (!db.TryFindIndex(_entity.Value.Id, out var index))
                return;

            if (!db.TryIndexOf(index + value, out var entity))
                return;

            _entity.Value = entity;
        }

        private void OnPrev() => AddDataIndex(-1);

        private void OnNext() => AddDataIndex(1);

        private void OnEntityChanged(SummonEntity data)
        {
            if (!data.IsValid)
                return;

            var db = Storage.db.summons;
            if (db.TryFindIndex(data.Id, out var entityIndex))
            {
                _prev.gameObject.SetActive(entityIndex > 0);
                _next.gameObject.SetActive(entityIndex < db.Length - 1);
            }
            else
            {
                _prev.gameObject.SetActive(false);
                _next.gameObject.SetActive(false);
            }

            _title.text = data.name;
            //_desc.text = data.comment;
            //_desc.gameObject.SetActive(!string.IsNullOrEmpty(_desc.text));

            bool TryAssign(int index, int costId, int coast, Button button, UIItemSlot slot,
                Action<int, int> onClick)
            {
                bool isEnough = HasEnoughItem(costId, coast, false);
                if (isEnough || index == 0)
                {
                    button.onClick.RemoveAllListeners();
                    
                    if(isEnough)
                        button.onClick.AddListener(() => onClick.Invoke(data.Id, index));

                    slot.Init(new Item { ItemId = costId, Quantity = coast })
                        .Forget();

                    slot.QuantityLabelColor = isEnough ? Color.white : Color.red;

                    return true;
                }

                return false;
            }

            bool x1Done = false;
            bool x10Done = false;
            var costItems = data.costItems;
            var perOnce = data.valuePerOnce;
            var perTenth = data.valuePerTenth;
            int i = Mathf.Min(costItems.Length, perOnce.Length, perTenth.Length) - 1;
            while (i >= 0)
            {
                int itemId = costItems[i];
                if (!x1Done)
                {
                    x1Done = TryAssign(i, itemId, perOnce[i], _x1, _itemForX1,
                        (id, index) => SummonOnce(id, index).Forget());
                }

                if (!x10Done)
                {
                    x10Done = TryAssign(i, itemId, perTenth[i], _x10, _itemForX10,
                        (id, index) => SummonTenth(id, index).Forget());
                }

                --i;
            }
        }

        private UniTask SummonOnce(int eventId, int costIndex)
            => Summon(eventId, costIndex, OnceTrigger, NetworkService.Summon.SummonOnce);

        private UniTask SummonTenth(int eventId, int costIndex)
            => Summon(eventId, costIndex, TenthTrigger, NetworkService.Summon.SummonTenth);

        private async UniTask Summon(int eventId, int costIndex, int animationHash,
            Func<int, int, UniTask<Result<Summon>>> method)
        {
            var result = await method.Invoke(eventId, costIndex);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
                return;
            }
            
            await TaskHelper.OnAnimationEnd(_animator, animationHash);

            OnSummoned(_result).Forget();
        }

        private async UniTaskVoid OnSummoned(Summon result)
        {
            var repository = Storage.userRepository;
            foreach (var summoned in result.list)
            {
                switch (summoned)
                {
                    case SummonedSoul soul:
                        repository.inventory.Add(new Item { ItemId = soul.Id, Quantity = soul.quantity });
                        break;
                    case SummonedUnit unit:
                        repository.characters.Add(new UnitInfo { id = unit.Id, lv = unit.lv, rate = unit.rate });
                        break;
                }
            }

            if (result.list.Count > 1)
            {
                var direction = await Context.popups
                    .OpenAsync<PopupSummonDirection>(result);

                await direction.DisplayTask;
            }

            Context.popups.Open<PopupSummonResult>(result);
        }

        private bool HasEnoughItem(int costId, int cost, bool openPopup = true)
        {
            var repo = Storage.userRepository;
            var currency = repo.currency;
            bool isEnough;
            switch (costId)
            {
                case Constants.GoldId:
                    isEnough = currency.gold.Value >= cost;
                    break;
                case Constants.PaidDiaId or Constants.FreeDiaId:
                    isEnough = currency.paidDia.Value + currency.freeDia.Value >= cost;
                    break;
                default:
                {
                    var item = repo.inventory.items.FirstOrDefault(x => x.ItemId == costId);
                    isEnough = item != null && item.Quantity >= cost;
                    break;
                }
            }

            if (!isEnough && openPopup)
                Context.popups.Open<PopupCommon>("재화 혹은 아이템 부족해유");

            return isEnough;
        }

        private Probability GetOrCreateProbability(int groupId)
        {
            if (!_probabilityCache.TryGetValue(groupId, out var data))
            {
                var groupEntities = Storage.db.summonGroups
                    .Map(groupId)
                    .ToList();

                var unitEntities = Storage.db.units
                    .Map(groupEntities.Select(x => x.unitId))
                    .ToList();

                data = groupEntities
                    .Join(unitEntities,
                        gEntity => gEntity.unitId,
                        uEntity => uEntity.Id,
                        (gEntity, uEntity) => new { gEntity.unitId, uEntity.name, gEntity.weight }
                    )
                    .ToDictionary(x => x.unitId, x => (x.name, x.weight));

                _probabilityCache[groupId] = data;
            }

            return data;
        }

        private string BuildInfoString(Probability cache)
        {
            bool isFirst = true;
            var sb = new StringBuilder();
            using var itr = cache.GetEnumerator();
            while (itr.MoveNext())
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.AppendLine();

                var current = itr.Current.Value;
                sb.Append($"{current.name} {current.weight:F3}%");
            }

            return sb.ToString();
        }
    }
}