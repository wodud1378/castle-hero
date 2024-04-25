using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace RGLabs.InGame.UI
{
    public class UICharacterList : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDisposable
    {
        [SerializeField] private float _dragThresholdTime = 0.2f;
        [SerializeField] private RectTransform _slotParent;
        [SerializeField] private AssetLabelReference _slotPrefab;

        public readonly ReactiveProperty<UICharacterSlot> selected = new(null);

        private UnitDB _db;
        private UserRepository _repository;

        private readonly List<UICharacterSlot> _slots = new();
        

        public async UniTask Init(UserRepository repository, UnitDB db)
        {
            _repository = repository;
            _db = db;

            var tasks = new List<UniTask>();
            foreach (var id in _repository.characters.Value)
            {
                if (!_db.TryFind(id, out var entity))
                    continue;

                tasks.Add(AddSlot(entity));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask<UICharacterSlot> AddSlot(UnitEntity entity)
        {
            var obj = await Addressables.InstantiateAsync(_slotPrefab, _slotParent);
            var slot = obj.GetComponent<UICharacterSlot>();
            await slot.Init(entity);

            _slots.Add(slot);

            return slot;
        }

        public void Dispose()
        {
            foreach (var slot in _slots)
            {
                slot.Dispose();
            }

            _slots.Clear();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 position = eventData.position;
            if (!_slotParent.rect.Contains(position))
                return;

            UICharacterSlot selectedSlot = null;
            float lastDistance = float.MaxValue;
            foreach (var slot in _slots)
            {
                float distance = Vector2.Distance(slot.transform.position, position);
                if (lastDistance < distance)
                {
                    selectedSlot = slot;
                    lastDistance = distance;
                }
            }

            if (selectedSlot == null)
                return;

            selected.Value = selectedSlot;
        }

        public void OnEndDrag(PointerEventData _)
        {
            selected.Value = null;
        }
    }
}