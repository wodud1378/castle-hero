using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Network.Shared;
using CastleHero.Utility;
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

        private IDBProvider _db;

        protected override void OnAwake()
        {
            base.OnAwake();
            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();
        }

        public void Init(ISummoned summoned)
        {
            switch (summoned)
            {
                case SummonedUnit unit:
                    itemObjects.ForEach(x => x.gameObject.SetActive(false));
                    label.text = string.Empty;
                    if (_db.Units.TryFind(unit.Id, out var uEntity))
                    {
                        SetCharacter(uEntity.uiPrefab).SafeForget();
                    }
                    break;
                case SummonedSoul soul:
                    itemObjects.ForEach(x => x.gameObject.SetActive(true));
                    if (_db.Items.TryFind(soul.Id, out var iEntity))
                    {
                        base.Init(iEntity.icon, $"x{soul.quantity}");
                    }
                    break;
            }
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
