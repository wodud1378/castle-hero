using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.DB;
using RGLabs.Network.Model;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public class UICharacterList : UIListAdapter<UICharacterSlot, UnitInfo>
    {
        private static readonly int UnFold = Animator.StringToHash("UnFold");
        private static readonly int Fold = Animator.StringToHash("Fold");

        [SerializeField] private Animator _animator;

        private UnitDB _db;
        
        protected override async UniTask SetItem(UICharacterSlot item, UnitInfo data) => await item.InitAsync(data, _db);

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

        private void OnDrawGizmosSelected()
        {
            var corners = new Vector3[4];
            itemRoot.GetWorldCorners(corners);

            var rootPos = itemRoot.position;
            var width = (corners[2] - corners[1]).x;
            var height = (corners[1] - corners[0]).y;
            rootPos.y -= height;
            var rect = new Rect(rootPos.x, rootPos.y, width, height);
            
            Gizmos.DrawWireCube(rect.center, rect.size);
        }
    }
}