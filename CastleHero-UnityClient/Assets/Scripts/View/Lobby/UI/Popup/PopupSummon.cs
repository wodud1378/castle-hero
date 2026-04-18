using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using CastleHero.View.Lobby.UI.Actions;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using Probability = System.Collections.Generic.Dictionary<int, (string name, float weight)>;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/PopupSummon.prefab")]
    public class PopupSummon : PopupBase
    {
        private static readonly int OnceTrigger = Animator.StringToHash("Action_1");
        private static readonly int TenthTrigger = Animator.StringToHash("Action_10");

        [FormerlySerializedAs("_title")]
        [SerializeField] private TMP_Text title;
        [FormerlySerializedAs("_desc")]
        [SerializeField] private TMP_Text desc;
        [FormerlySerializedAs("_info")]
        [SerializeField] private Button info;
        [FormerlySerializedAs("_prev")]
        [SerializeField] private Button prev;
        [FormerlySerializedAs("_next")]
        [SerializeField] private Button next;
        [FormerlySerializedAs("_x1")]
        [SerializeField] private Button x1;
        [FormerlySerializedAs("_itemForX1")]
        [SerializeField] private UIItemSlot itemForX1;
        [FormerlySerializedAs("_x10")]
        [SerializeField] private Button x10;
        [FormerlySerializedAs("_itemForX10")]
        [SerializeField] private UIItemSlot itemForX10;

        private readonly Dictionary<int, Probability> _probabilityCache = new();
        private readonly ReactiveProperty<SummonEntity> _entity = new();

        private IDBProvider _db;
        private IPopupManager _popups;
        private SummonAction _action;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();
            _popups = sl.Get<IPopupManager>();
            _action = sl.Get<SummonAction>();

            this.SubscribeButton(info, () => OpenInfo().SafeForget());
            this.SubscribeButton(prev, OnPrev);
            this.SubscribeButton(next, OnNext);
        }

        public override UniTask Open() => Open(_db.Summons[0]);

        public override UniTask Open(params object[] parameters)
        {
            if (parameters[0] is not SummonEntity entity)
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

            var popup = await _popups
                .OpenAsync<PopupCommon>(infoString);

            var buttonRect = (info.transform as RectTransform)!;
            var popupRect = (popup.transform as RectTransform)!;

            popupRect.Attach(buttonRect, new Vector2(0f, 1f));
        }

        private void AddDataIndex(int value)
        {
            var db = _db.Summons;
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

            var db = _db.Summons;
            if (db.TryFindIndex(data.Id, out var entityIndex))
            {
                prev.gameObject.SetActive(entityIndex > 0);
                next.gameObject.SetActive(entityIndex < db.Length - 1);
            }
            else
            {
                prev.gameObject.SetActive(false);
                next.gameObject.SetActive(false);
            }

            title.text = data.name;

            bool TryAssign(int index, int costId, int coast, Button button, UIItemSlot slot,
                Action<int, int> onClick)
            {
                bool isEnough = _action.HasEnoughItem(costId, coast, false);
                if (isEnough || index == 0)
                {
                    button.onClick.RemoveAllListeners();

                    if (isEnough)
                        button.onClick.AddListener(() => onClick.Invoke(data.Id, index));

                    slot.Init(new Item { ItemId = costId, Quantity = coast });

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
                    x1Done = TryAssign(i, itemId, perOnce[i], x1, itemForX1,
                        (id, index) => SummonOnce(id, index).SafeForget());
                }

                if (!x10Done)
                {
                    x10Done = TryAssign(i, itemId, perTenth[i], x10, itemForX10,
                        (id, index) => SummonTenth(id, index).SafeForget());
                }

                --i;
            }
        }

        private async UniTask SummonOnce(int eventId, int costIndex)
        {
            var summon = await _action.SummonOnce(eventId, costIndex);
            if (summon == null) return;

            await TaskHelper.OnAnimationEnd(_animator, OnceTrigger);
            OnSummoned(summon).SafeForget();
        }

        private async UniTask SummonTenth(int eventId, int costIndex)
        {
            var summon = await _action.SummonTenth(eventId, costIndex);
            if (summon == null) return;

            await TaskHelper.OnAnimationEnd(_animator, TenthTrigger);
            OnSummoned(summon).SafeForget();
        }

        private async UniTaskVoid OnSummoned(Summon result)
        {
            _action.ApplySummonResult(result);

            if (result.list.Count > 1)
            {
                var direction = await _popups
                    .OpenAsync<PopupSummonDirection>(result);

                await direction.DisplayTask;
            }

            _popups.Open<PopupSummonResult>(result);
        }

        private Probability GetOrCreateProbability(int groupId)
        {
            if (!_probabilityCache.TryGetValue(groupId, out var data))
            {
                var groupEntities = _db.SummonGroups
                    .Map(groupId)
                    .ToList();

                var unitEntities = _db.Units
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
