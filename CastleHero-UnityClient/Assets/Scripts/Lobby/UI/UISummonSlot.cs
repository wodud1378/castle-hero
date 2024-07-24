using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Network.Shared;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Lobby.UI
{
    public class UISummonSlot : UISlot
    {
        [SerializeField] private RectTransform _prefabRoot;
        [SerializeField] private SkeletonGraphic _skeleton;

        public UniTask Init(ISummoned summoned)
        {
            switch (summoned)
            {
                case SummonedUnit unit :
                    _skeleton.gameObject.SetActive(true);
                    icon.gameObject.SetActive(false);
                    label.text = string.Empty;
                    if (Storage.db.units.TryFind(unit.Id, out var uEntity))
                    {
                        return SetCharacter(uEntity.uiPrefab);
                    }
                    break;
                case SummonedSoul soul :
                    _skeleton.gameObject.SetActive(false);
                    icon.gameObject.SetActive(true);
                    if (Storage.db.items.TryFind(soul.Id, out var iEntity))
                    {
                        return base.Init(iEntity.icon, $"x{soul.quantity}");
                    }
                    break;
            }
            
            return UniTask.CompletedTask;
        }

        private async UniTask SetCharacter(string dataPath)
        {
            _prefabRoot.gameObject.SetActive(false);
            
            await Addressables.InstantiateAsync(dataPath, _prefabRoot);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            _prefabRoot.gameObject.SetActive(true);
        }
    }
}