#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace RGLabs.InGame.Utility
{
    public class TestDrawer : MonoBehaviour
    {
        public float range;
        public Transform from;
        public Collider2D target;
        
        private void OnDrawGizmos()
        {
            if (this.from == null || target == null)
                return;
            
            Vector2 from = this.from.position;
            var closest = target.ClosestPoint(from);
            var ranged = (closest - from).normalized * range;
            var final = closest - ranged;
            
            Gizmos.DrawLine(from, final);
        }
    }
}
#endif