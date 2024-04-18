using System;
using RGLabs.Common;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.System.Wave;
using RGLabs.InGame.Utility;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RGLabs.InGame.Behaviours.Wave
{
    public class SpawnSystemBehaviour : SystemBehaviour
    {
        [SerializeField] private int _id;
        [SerializeField] private float _size;

        public int Id => _id;

        private CreationHelper _creationHelper;
        private DataStream<ReleaseEvent> _releaseStream;
        private IUnitFactory _factory;

        protected override void OnInit()
        {
            GetCorners(out var cornerA, out var cornerB);

            _factory = new DefaultUnitFactory(InGameContext.pools);
            _creationHelper = new CreationHelper(_id, cornerA, cornerB);
            _releaseStream = InGameContext.streams.release;
        }

        protected override void OnUpdate() => _creationHelper.SetUpBuffers(Create);

        private void GetCorners(out Vector2 a, out Vector2 b)
        {
            var tr = transform;
            var pos = (Vector2)tr.position;
            var angle = tr.localEulerAngles.z;
            var halfSize = _size * 0.5f;
            a = new Vector2(pos.x - halfSize, pos.y).Rotate(pos, angle);
            b = new Vector2(pos.x + halfSize, pos.y).Rotate(pos, angle);
        }

        private async void Create(UnitCreation request)
        {
            var position = request.position;
            var unit = await _factory.Create<UnitBehaviour>(request.entity, position);
            unit.defaultDestination = unit.ClosestPoint(position, InGameContext.camp.castle);
            unit.autoRelease = false;
            unit.OnDead += OnUnitDead;
        }

        private void OnUnitDead(UnitBehaviour unit)
        {
            _releaseStream.Emit(new ReleaseEvent { unit = unit });

            unit.OnDead -= OnUnitDead;
        }

        private void OnDestroy()
        {
            _creationHelper = null;
            _factory = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var split = name.Split('_');
            if (split.Length < 2)
                return;

            if (!int.TryParse(split[1], out int id))
                return;

            _id = id;
        }

        private void OnDrawGizmos()
        {
            DrawArea();
        }

        private void DrawArea()
        {
            var tr = transform;
            var pos = (Vector2)tr.position;
            var points = new Vector2[4];

            var halfSize = _size * 0.5f;
            var angle = tr.localEulerAngles.z;
            points[0] = new Vector2(pos.x - halfSize, pos.y + 1f).Rotate(pos, angle);
            points[1] = new Vector2(pos.x + halfSize, pos.y + 1f).Rotate(pos, angle);
            points[2] = new Vector2(pos.x + halfSize, pos.y).Rotate(pos, angle);
            points[3] = new Vector2(pos.x - halfSize, pos.y).Rotate(pos, angle);

            Debug.DrawLine(points[0], points[1], Color.cyan);
            Debug.DrawLine(points[1], points[2], Color.cyan);
            Debug.DrawLine(points[2], points[3], Color.cyan);
            Debug.DrawLine(points[3], points[0], Color.cyan);

            Vector2 center = default;
            foreach (var point in points)
            {
                center += point;
            }

            center /= 4;

            var style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.cyan }
            };

            Handles.Label(center, $"{_id}", style);
        }
#endif
    }
}