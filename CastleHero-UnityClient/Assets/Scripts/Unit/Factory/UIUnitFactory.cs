using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace RGLabs.Unit.Factory
{
    public class UIUnitFactory : IUnitFactory
    {
        private readonly int _layerId = SortingLayer.NameToID("UI");

        public async UniTask<T> Create<T>(UnitEntity entity, Vector2 position) where T : UnitBehaviour
        {
            var obj = await Addressables.InstantiateAsync(entity.prefab);
            if (!obj.TryGetComponent(out T unit))
            {
                Addressables.ReleaseInstance(obj);
                return null;
            }

            if (unit.TryGetComponent(out SortingGroup sortingGroup))
            {
                sortingGroup.sortingLayerID = _layerId;
            }
            
            unit.transform.position = position;
            unit.canAttack = false;
            unit.canMove = false;
            return unit;
        }
    }
}