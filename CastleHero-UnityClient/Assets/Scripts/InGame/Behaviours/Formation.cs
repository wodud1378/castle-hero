using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Common;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public struct UnitSet
    {
        public UnitBehaviour unit;
        public Vector2 position;
    }

    public struct SavedUnit
    {
        public readonly int id;
        public readonly Vector2 position;

        public SavedUnit(UnitSet unitSet)
        {
            id = unitSet.unit.Id;
            position = unitSet.position;
        }
    }

    public class Formation : MonoBehaviour
    {
        [field: SerializeField] public float Radius { get; private set; }

        [SerializeField] private int _defaultCastleId;
        
        private readonly Collider2D[] _buffer = new Collider2D[Constants.SpawnBufferSize];

        private UnitDB _db;
        private InGameRepository _repository;
        private IUnitFactory _factory;

        public async UniTask Init(UnitDB db, InGameRepository repository, IUnitFactory factory)
        {
            _db = db;
            _repository = repository;
            _factory = factory;

            await LoadCastle();
            await LoadSavedUnits();
        }

        public bool TryRegister(UnitBehaviour unit)
        {
            if (!unit.IsValid())
                return false;
            
            if (!IsValid(unit.Collider))
                return false;

            Register(unit);
            return true;
        }

        public bool IsValid(Collider2D col)
        {
            int layer = col.gameObject.layer;
            var layerMask = new LayerMask { value = 1 << layer };
            var distance = Vector2.Distance(col.transform.position, transform.position);
            if (distance > Radius || distance < 1f)
                return false;

            return Physics2D.OverlapCollider(col, new ContactFilter2D { layerMask = layerMask }, _buffer) <= 0;
        }

        private void Register(UnitBehaviour unit)
        {
            var array = _repository.characterSet.Value;
            int index = Array.FindIndex(array, (x) => x.unit.Id == unit.Id);
            if (!index.IsValidIndex(array))
            {
                int length = array.Length;
                index = length;
                Array.Resize(ref array, length + 1);
            }

            array[index] = new UnitSet
            {
                unit = unit,
                position = unit.transform.position,
            };

            _repository.characterSet.Value = array;
            _repository.SaveCharacters();
        }

        private async UniTask LoadCastle()
        {
            if (!_db.TryFind(_repository.savedCastle.Value, out var entity))
            {
                if (!_db.TryFind(_defaultCastleId, out entity))
                    return;
            }

            _repository.castle.Value = await CreateUnit(entity, Vector2.zero);
        }
        
        private async UniTask LoadSavedUnits()
        {
            var tasks = new List<UniTask>();
            var saved = _repository.savedCharacters.Value;
            if (saved == null)
                return;
            
            foreach (var set in _repository.savedCharacters.Value)
            {
                if (!_db.TryFind(set.id, out var entity))
                    continue;

                tasks.Add(CreateCharacter(entity, set.position));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask CreateCharacter(UnitEntity entity, Vector2 position)
        {
            var unit = await CreateUnit(entity, position);
            Register(unit);
        }

        private async UniTask<UnitBehaviour> CreateUnit(UnitEntity entity, Vector2 position)
        {
            var unit = await entity.Create<UnitBehaviour>(position, _factory);
            unit.defaultDestination = position;
            unit.canMove = false;
            unit.canAttack = false;
            return unit;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}