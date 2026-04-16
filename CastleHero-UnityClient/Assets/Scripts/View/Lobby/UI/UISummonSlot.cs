using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Network.Shared;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI
{
    public class UISummonSlot : UISlot
    {
        [FormerlySerializedAs("_itemObjects")]
        [SerializeField] private List<GameObject> itemObjects;
        [FormerlySerializedAs("_prefabRoot")]
        [SerializeField] private RectTransform prefabRoot;

        public UniTask Init(ISummoned summoned)
        {
            switch (summoned)
            {
                case SummonedUnit unit:
                    itemObjects.ForEach(x => x.gameObject.SetActive(false));
                    label.text = string.Empty;
                    if (ServiceLocator.Get<IDBProvider>().Units.TryFind(unit.Id, out var uEntity))
                    {
                        return SetCharacter(uEntity.uiPrefab);
                    }
                    break;
                case SummonedSoul soul:
                    itemObjects.ForEach(x => x.gameObject.SetActive(true));
                    if (ServiceLocator.Get<IDBProvider>().Items.TryFind(soul.Id, out var iEntity))
                    {
                        return base.Init(iEntity.icon, $"x{soul.quantity}");
                    }
                    break;
            }

            return UniTask.CompletedTask;
        }

        private async UniTask SetCharacter(string dataPath)
        {
            prefabRoot.gameObject.SetActive(false);

            await Addressables.InstantiateAsync(dataPath, prefabRoot);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            prefabRoot.gameObject.SetActive(true);
        }
    }
}
