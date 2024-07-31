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
using RGLabs.Network.Service.Summon;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Cache = System.Collections.Generic.Dictionary<int, (string name, float weight)>;

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

        public override UniTask Open() => Open(0);

        private readonly Dictionary<int, Cache> _propCache = new();
        private readonly ReactiveProperty<SummonEntity> _entity = new();
        private readonly SummonService _service = new();

        private SummonResult _result;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_info, () => OpenInfo().Forget());
            this.SubscribeButton(_prev, OnPrev);
            this.SubscribeButton(_next, OnNext);
        }

        public override UniTask Open(params object[] parameters)
        {
            int index = (int)parameters[0];
            if (!Storage.db.summons.TryIndexOf(index, out var entity))
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
            var data = GetOrCreateFromCache(_entity.Value.groupId);
            var infoString = BuildInfoString(data);

            var popup = await Context.popupManager
                .OpenAsync<PopupCommon>(infoString);

            var buttonRect = (_info.transform as RectTransform)!;
            var popupRect = (popup.transform as RectTransform)!;

            popupRect.AttachThrough(buttonRect, 0f, 1f);
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

            bool TryAssign(int index, int coastId, int coast, Button button, UIItemSlot slot,
                Action<int, int, int> onClick)
            {
                bool isEnough = CheckInventory(coastId, coast, false);
                if (isEnough ||
                    index == 0)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => onClick.Invoke(data.Id, coastId, coast));

                    slot.Init(new Item { ItemId = coastId, Quantity = coast })
                        .Forget();

                    slot.QuantityLabelColor = isEnough ? Color.white : Color.red;

                    return true;
                }

                return false;
            }

            bool x1Done = false;
            bool x10Done = false;
            var coastItems = data.item;
            var coastPerOnce = data.valuePerOnce;
            var coastPerTenth = data.valuePerTenth;
            int i = Mathf.Min(coastItems.Length, coastPerOnce.Length, coastPerTenth.Length) - 1;
            while (i >= 0)
            {
                int itemId = coastItems[i];
                if (!x1Done)
                {
                    x1Done = TryAssign(i, itemId, coastPerOnce[i], _x1, _itemForX1,
                        (eId, cId, c) => SummonOnce(eId, cId, c).Forget());
                }

                if (!x10Done)
                {
                    x10Done = TryAssign(i, itemId, coastPerTenth[i], _x10, _itemForX10,
                        (eid, cId, c) => SummonTenth(eid, cId, c).Forget());
                }

                --i;
            }
        }

        private UniTask SummonOnce(int eventId, int coastId, int coast)
            => Summon(eventId, coastId, coast, OnceTrigger, _service.SummonOnce);

        private UniTask SummonTenth(int eventId, int coastId, int coast)
            => Summon(eventId, coastId, coast, TenthTrigger, _service.SummonTenth);

        private async UniTask Summon(int eventId, int coastId, int coast, int animationHash,
            Func<int, int, UniTask<SummonResult>> method)
        {
            if (!CheckInventory(coastId, coast))
                return;

            var networkTask = SetResult(() => method.Invoke(eventId, coastId));
            var uiTask = TaskHelper.OnAnimationEnd(_animator, animationHash);

            await UniTask.WhenAll(networkTask, uiTask);

            OnSummoned(_result).Forget();
        }

        private async UniTask SetResult(Func<UniTask<SummonResult>> method) => _result = await method.Invoke();

        private async UniTaskVoid OnSummoned(SummonResult result)
        {
            var repository = Storage.userRepository;
            foreach (var summoned in result.summoneds)
            {
                switch (summoned)
                {
                    case SummonedSoul soul:
                        repository.Add(new Item { ItemId = soul.Id, Quantity = soul.quantity });
                        break;
                    case SummonedUnit unit:
                        repository.Add(new UnitInfo { id = unit.Id, lv = unit.lv, rate = unit.rate });
                        break;
                }
            }

            if (result.summoneds.Count > 1)
            {
                var direction = await Context.popupManager
                    .OpenAsync<PopupSummonDirection>(result);

                await direction.DisplayTask;
            }

            Context.popupManager
                .OpenAsync<PopupSummonResult>(result)
                .Forget();
        }

        private bool CheckInventory(int coastId, int coast, bool openPopup = true)
        {
            var repo = Storage.userRepository;
            bool isEnough;
            if (coastId == Constants.GoldId)
                isEnough = repo.gold.Value >= coast;
            else if (coastId == Constants.PaidDiaId || coastId == Constants.FreeDiaId)
                isEnough = repo.paidDia.Value + repo.freeDia.Value >= coast;
            else
            {
                var item = repo.items.FirstOrDefault(x => x.ItemId == coastId);
                isEnough = item != null && item.Quantity >= coast;
            }

            if (!isEnough && openPopup)
            {
                Context.popupManager
                    .OpenAsync<PopupCommon>("재화 혹은 아이템 부족해유")
                    .Forget();
            }

            return isEnough;
        }

        private Cache GetOrCreateFromCache(int groupId)
        {
            if (!_propCache.TryGetValue(groupId, out var data))
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

                _propCache[groupId] = data;
            }

            return data;
        }

        private string BuildInfoString(Cache cache)
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