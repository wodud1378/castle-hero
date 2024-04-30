using RGLabs.Data.DB;
using UnityEngine;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Factory;
using RGLabs.Utility;

namespace RGLabs.InGame.System.Wave
{
    public class SpawnArea : IUpdate
    {
        public readonly int id;

        private readonly Vector2 _position;
        private readonly float _size;
        private readonly float _angle;
        private readonly IUnitFactory _factory;
        private readonly CreationHelper _creationHelper;
        private readonly UnitBehaviour _castle;
        
        public SpawnArea(int id, Vector2 position, float size, float angle, UnitDB db, IUnitFactory factory, UnitBehaviour castle)
        {
            this.id = id;

            _position = position;
            _size = size;
            _angle = angle;
            _factory = factory;
            _castle = castle;
            
            GetCorners(out var cornerA, out var cornerB);

            _creationHelper = new CreationHelper(id, db, cornerA, cornerB);
        }

        public void ProcessUpdate(float _) => _creationHelper.SetUpBuffers(Create);
        
        private void GetCorners(out Vector2 a, out Vector2 b)
        {
            var halfSize = _size * 0.5f;
            a = new Vector2(_position.x - halfSize, _position.y).Rotate(_position, _angle);
            b = new Vector2(_position.x + halfSize, _position.y).Rotate(_position, _angle);
        }
        
        private async void Create(UnitCreation request)
        {
            var position = request.position;
            var unit = await _factory.Create<UnitBehaviour>(request.entity, position);
            unit.defaultDestination = unit.ClosestPoint(_castle);
            unit.canMove = true;
            unit.canAttack = true;
            unit.autoRelease = false;
            unit.OnDead += OnUnitDead;
        }

        private void OnUnitDead(UnitBehaviour unit)
        {
            new ReleaseEvent { unit = unit }.Publish();

            unit.OnDead -= OnUnitDead;
        }
    }
}