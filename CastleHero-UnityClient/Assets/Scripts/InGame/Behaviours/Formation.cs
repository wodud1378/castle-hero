using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Common;
using RGLabs.InGame.System.UnitFactory;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public struct UnitSetUp
    {
        public Vector2 position;
        public UnitBehaviour unit;
    }
    
    public class SettingField : MonoBehaviour
    {
        [SerializeField] private float _radius;
        [SerializeField] private LayerMask _layerMask;

        private readonly Collider2D[] _buffer = new Collider2D[Constants.SpawnBufferSize];
        private readonly List<UnitSetUp> _setupList = new();

        private IUnitFactory _factory;

        public void Init()
        {
            _setupList.Clear();
        }
        
        public bool TryRegister(UnitBehaviour unit)
        {
            if (!IsValid(unit.Collider))
                return false;
            
            _setupList.Add(new UnitSetUp
            {
                position = unit.transform.position,
                unit = unit
            });
            
            return true;
        }
        
        public bool IsValid(Collider2D col)
        {
            var filter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = true,
                layerMask = _layerMask,
            };
            
            return Physics2D.OverlapCollider(col, filter, _buffer) <= 0;
        }

        private void Save()
        {
            foreach (var setup in _setupList)
            {
                
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(transform.position, _radius);
        }
    }
}