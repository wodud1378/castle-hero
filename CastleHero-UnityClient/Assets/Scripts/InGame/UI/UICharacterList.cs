using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.Data.User;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace RGLabs.InGame.UI
{
    public class UICharacterList : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDisposable
    {
        [SerializeField] private RectTransform _slotParent;
        [SerializeField] private AssetReference _slotPrefab;

        public readonly ReactiveProperty<UICharacterSlot> selected = new(null);
        
        private readonly List<UICharacterSlot> _slots = new();

        private UserRepository _repository;
        
        public async UniTask Init(UserRepository repository, UnitDB db)
        {
            _repository = repository;

            var tasks = new List<UniTask>();
            foreach (var character in _repository.characters.Value)
            {
                tasks.Add(AddSlot(character, db));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask<UICharacterSlot> AddSlot(Character character, UnitDB db)
        {
            var obj = await Addressables.InstantiateAsync(_slotPrefab, _slotParent);
            var slot = obj.GetComponent<UICharacterSlot>();
            await slot.InitAsync(character, db);

            _slots.Add(slot);

            return slot;
        }

        public void Dispose()
        {
            foreach (var slot in _slots)
            {
                slot.Dispose();
                Addressables.ReleaseInstance(slot.gameObject);
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