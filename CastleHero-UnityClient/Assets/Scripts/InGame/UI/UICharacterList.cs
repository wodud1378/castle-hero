using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.Data.User;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UICharacterList : MonoBehaviour
    {
        private static readonly int UnFold = Animator.StringToHash("UnFold");
        private static readonly int Fold = Animator.StringToHash("Fold");

        [field: SerializeField] public RectTransform SlotParent { get; private set; }

        [SerializeField] private Animator _animator;
        [SerializeField] private AssetReference _slotPrefab;
        
        public bool IsOpen { get; private set; }
        
        private readonly List<UICharacterSlot> _slots = new();

        public async UniTask Init(Character[] characters, UnitDB db)
        {
            var tasks = new List<UniTask>();
            foreach (var character in characters)
            {
                tasks.Add(AddSlot(character, db));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask<UICharacterSlot> AddSlot(Character character, UnitDB db)
        {
            var obj = await Addressables.InstantiateAsync(_slotPrefab, SlotParent);
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

        public UICharacterSlot GetSlot(Vector2 position)
        {
            var corners = new Vector3[4];
            SlotParent.GetWorldCorners(corners);

            var rootPos = SlotParent.position;
            var width = (corners[2] - corners[1]).x;
            var height = (corners[1] - corners[0]).y;
            var rect = new Rect(rootPos.x, rootPos.y, width, height);

            if (rect.Contains(position))
                return null;

            UICharacterSlot selectedSlot = null;
            float lastDistance = float.MaxValue;
            foreach (var slot in _slots)
            {
                float distance = Vector2.Distance(slot.transform.position, position);
                if (lastDistance > distance)
                {
                    selectedSlot = slot;
                    lastDistance = distance;
                }
            }

            return selectedSlot;
        }

        public void Open()
        {
            IsOpen = true;

            _animator.SetTrigger(UnFold);
        }

        public void Close()
        {
            IsOpen = false;

            _animator.SetTrigger(Fold);
        }

        #region Animation Events.

        public void OnClosed()
        {
            Dispose();
        }

        #endregion
    }
}