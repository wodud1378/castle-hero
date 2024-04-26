using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.Data.User;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UICharacterList : MonoBehaviour
    {
        public Action<UICharacterSlot> OnSlotCreated;
        
        private static readonly int UnFold = Animator.StringToHash("UnFold");      
        private static readonly int Fold = Animator.StringToHash("Fold");

        [SerializeField] private Animator _animator;
        [SerializeField] private RectTransform _slotParent;
        [SerializeField] private AssetReference _slotPrefab;
        [SerializeField] private Button _close;

        public readonly ReactiveProperty<UICharacterSlot> selected = new(null);
        
        private readonly List<UICharacterSlot> _slots = new();

        private UserRepository _repository;

        private void Awake()
        {
            _close
                .OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(this);
        }

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
            OnSlotCreated?.Invoke(slot);

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

            OnSlotCreated = null;
        }

        public UICharacterSlot GetSlot(Vector2 position)
        {
            if (!_slotParent.rect.Contains(position))
                return null;

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

            return selectedSlot;
        }
        
        public void Open() => _animator.SetTrigger(UnFold);

        public void Close() => _animator.SetTrigger(Fold);

        #region Animation Events.
        public void OnClosed()
        {
            Dispose();
        }
        #endregion
    }
}