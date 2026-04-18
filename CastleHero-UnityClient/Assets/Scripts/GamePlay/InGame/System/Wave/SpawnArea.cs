using System;
using CastleHero.Common.Behaviours;
using CastleHero.Data.Factory;
using CastleHero.Data.DB;
using CastleHero.GamePlay.InGame.System.Wave.Creation;
using CastleHero.GamePlay.Unit.Factory;
using UnityEngine;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;

namespace CastleHero.GamePlay.InGame.System.Wave
{
    public class SpawnArea : IUpdate, IDisposable
    {
        public int TotalSpawn { get; private set; }

        public readonly int id;

        private readonly Vector2 _position;
        private readonly float _size;
        private readonly float _angle;
        private readonly UnitFactory _factory;
        private readonly ICreationHelper _creationHelper;
        private readonly UnitActor _castle;

        public SpawnArea(int id, Vector2 position, float size, float angle, UnitDB db, UnitFactory factory, UnitActor castle)
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

        public void Dispose() => _creationHelper.Dispose();

        public void ProcessUpdate(float _) => _creationHelper.SetUpCreations(Create);

        private void GetCorners(out Vector2 a, out Vector2 b)
        {
            var halfSize = _size * 0.5f;
            a = new Vector2(_position.x - halfSize, _position.y).Rotate(_position, _angle);
            b = new Vector2(_position.x + halfSize, _position.y).Rotate(_position, _angle);
        }

        private void Create(UnitCreation request)
        {
            var position = request.position;
            var data = new UnitCreationData(
                new UnitInfo { id = request.id, lv = request.lv, rate = 0 },
                position);
            if (_factory.Create(data) is not UnitActor unit)
                return;
            unit.UnitState.OnRest.Value = false;
            unit.Combat.Movement.Default = Vector2.zero;
            unit.autoRelease = false;
            unit.OnDead.Take(1).Subscribe(OnUnitDead);

            ++TotalSpawn;
        }

        private void OnUnitDead(UnitActor unit)
        {
            new ReleaseEvent { unit = unit }.Publish();
        }
    }
}