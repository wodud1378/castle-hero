using System.Collections.Generic;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Unit.Components
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Detecting : MonoBehaviour
    {
        [SerializeField] private List<string> _allowTags;
        [SerializeField] private int _maxTarget = 10;

        public List<GameUnit> Targets = new();

        public bool HasDetected => Targets.Count > 0;

        public GameUnit this[int index]
        {
            get
            {
                if (index.IsOutOfRange(Targets))
                    return null;

                return Targets[index];
            }
        }

        private readonly Queue<GameUnit> _addBuffer = new();
        private readonly Queue<GameUnit> _removeBuffer = new();

        private void Update()
        {
            Targets.RemoveAll((e) => !e.IsValid());

            while (_addBuffer.Count > 0)
            {
                var unit = _addBuffer.Dequeue();
                if (unit.IsValid() && Targets.Count < _maxTarget)
                    Targets.Add(unit);
            }

            while (_removeBuffer.Count > 0)
            {
                var unit = _removeBuffer.Dequeue();
                if (Targets.Contains(unit))
                    Targets.Remove(unit);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!_allowTags.Contains(other.tag))
                return;
            
            if (!other.gameObject.TryGetComponent(out GameUnit found))
                return;

            if (!found.IsValid())
                return;

            _removeBuffer.Enqueue(found);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_allowTags.Contains(other.tag))
                return;
            
            if (_addBuffer.Count >= _maxTarget)
                return;

            if (!other.gameObject.TryGetComponent(out GameUnit found))
                return;

            _addBuffer.Enqueue(found);
        }
    }
}