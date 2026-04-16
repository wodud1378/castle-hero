using CastleHero.Common.Behaviours;
using CastleHero.Data.Factory;
using CastleHero.Data.DB;
using CastleHero.GamePlay.InGame.System.Wave.Creation;
using UnityEngine;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Utility;

using Cysharp.Threading.Tasks;
namespace CastleHero.GamePlay.InGame.System.Wave
{
    public class SpawnArea : IUpdate
    {
        public int TotalSpawn { get; private set; }
        
        public readonly int id;

        private readonly Vector2 _position;
        private readonly float _size;
        private readonly float _angle;
        private readonly IUnitFactory _factory;
        private readonly ICreationHelper _creationHelper;
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

        public void ProcessUpdate(float _) => _creationHelper.SetUpCreations(req => Create(req).Forget());
        
        private void GetCorners(out Vector2 a, out Vector2 b)
        {
            var halfSize = _size * 0.5f;
            a = new Vector2(_position.x - halfSize, _position.y).Rotate(_position, _angle);
            b = new Vector2(_position.x + halfSize, _position.y).Rotate(_position, _angle);
        }
        
        private async UniTask Create(UnitCreation request)
        {
            var position = request.position;
            if (await _factory.Create(request.id, request.lv, 0, position) is not UnitBehaviour unit)
                return;
            unit.Core.OnRest.Value = false;
            unit.Core.Movement.Default = Vector2.zero;
            unit.autoRelease = false;
            unit.OnDead += OnUnitDead;

            ++TotalSpawn;
        }

        private void OnUnitDead(UnitBehaviour unit)
        {
            new ReleaseEvent { unit = unit }.Publish();

            unit.OnDead -= OnUnitDead;
        }
    }
}