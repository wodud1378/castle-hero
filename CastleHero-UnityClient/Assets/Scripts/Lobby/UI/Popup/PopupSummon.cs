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
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Network.Service.Summon;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

using Cache = System.Collections.Generic.Dictionary<int, (string name, float weight)>;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Gacha/Gacha.prefab")]
    public class PopupSummon : PopupBase
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _info;
        [SerializeField] private Button _next;
        [SerializeField] private Button _prev;
        [SerializeField] private Button _x1;
        [SerializeField] private UIItemSlot _itemForX1;
        [SerializeField] private Button _x10;
        [SerializeField] private UIItemSlot _itemForX10;
        [SerializeField] private UISummonedList _result;

        public override UniTask Open() => Open(0);

        private readonly Dictionary<int, Cache> _propCache = new();
        private readonly ReactiveProperty<SummonEntity> _entity = new();
        private readonly SummonService _service = new();

        protected override void OnAwake()
        {
            base.OnAwake();
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

        private void OpenInfo()
        {
            var data = GetOrCreateFromCache(_entity.Value.groupId);
            var infoString = BuildInfoString(data);

            Context.popupManager
                .OpenAsync<PopupCommon>(infoString)
                .Forget();
        }

        private void OnEntityChanged(SummonEntity data)
        {
            _title.text = data.comment;
            bool TryAssign(int index, int coastId, int coast, Button button, UIItemSlot slot, Action<int, int, int> onClick)
            {
                if (CheckInventory(coastId, coast, false) ||
                    index == 0)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(()=> onClick.Invoke(data.Id, coastId, coast));
                    
                    var item = Storage.userRepository.items.FirstOrDefault(x => x.ItemId == coastId);
                    slot.Init(item)
                        .Forget();
                    
                    return true;
                }

                return false;
            }
            
            bool x1Done = false;
            bool x10Done = false;
            var coastItems = data.item;
            var coastPerOnce = data.valuePerOnce;
            var coastPerTenth = data.valuePerTenth;
            int i = Mathf.Min(coastItems.Length, coastPerOnce.Length, coastPerTenth.Length);
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

        private async UniTask SummonOnce(int eventId, int coastId, int coast)
        {
            if (!CheckInventory(coastId, coast))
                return;

            var result = await _service.SummonOnce(eventId, coastId);
            
            await _result.Init(result.summoneds);
        }

        private async UniTask SummonTenth(int eventId, int coastId, int coast)
        {
            if (!CheckInventory(coastId, coast))
                return;

            var result = await _service.SummonOnce(eventId, coastId);
            
            await _result.Init(result.summoneds);
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
                sb.Append($"{current.name} {current.weight:F3}");
            }

            return sb.ToString();
        }
    }
}