using RGLabs.Common.Flow;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Common.UI.Popup
{
    public abstract class PopupBase : MonoBehaviour, IBackButtonListener
    {
        [SerializeField] private Button _close;

        private void Awake()
        {
            _close
                .OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(this);
        }

        public void Close()
        {
            // TODO : 애니메이션 연결
            
            Closed();
        }
        
        public virtual bool OnProcessBack()
        {
            Close();
            
            return true;
        }
        
        // TODO : 애니메이션 연결
        #region Animation Events.

        public void Closed()
        {
            Addressables.ReleaseInstance(gameObject);
        }

        #endregion
    }
}