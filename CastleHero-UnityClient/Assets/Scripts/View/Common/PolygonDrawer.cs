using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Common
{
    [RequireComponent(typeof(MeshFilter))]
    public class PolygonDrawer : MonoBehaviour
    {
        [FormerlySerializedAs("_renderer")]
        [SerializeField] private MeshRenderer renderer;
        [FormerlySerializedAs("_mesh")]
        [SerializeField] private Mesh mesh;

        public Color Color { set => ApplyColor(value); }

        public int polygon;
        public float size;

        private Vector3[] _vertices;
        private int[] _triangles;

        private int _propertyId;
        private MaterialPropertyBlock _propertyBlock;

        private bool _initialized = false;

        public void Init()
        {
            if (_initialized)
                return;

            _propertyBlock = new MaterialPropertyBlock();
            _propertyId = Shader.PropertyToID("_Color");
            renderer.SetPropertyBlock(_propertyBlock);

            UpdateMesh();
            _initialized = true;
        }

        public void UpdateSize(float size)
        {
            this.size = size;

            UpdateMesh();
        }

        private void ApplyColor(Color color)
        {
            _propertyBlock.SetColor(_propertyId, color);
            renderer.SetPropertyBlock(_propertyBlock);
        }

        private void UpdateMesh()
        {
            _vertices = new Vector3[polygon + 1];

            _vertices[0] = new Vector3(0, 0, 0);
            for (int i = 1; i <= polygon; i++)
            {
                float angle = -i * (Mathf.PI * 2.0f) / polygon;

                _vertices[i] = (new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * size);
            }

            _triangles = new int[3 * polygon];
            for (int i = 0; i < polygon - 1; i++)
            {
                _triangles[i * 3] = 0;
                _triangles[i * 3 + 1] = i + 1;
                _triangles[i * 3 + 2] = i + 2;
            }

            _triangles[3 * polygon - 3] = 0;
            _triangles[3 * polygon - 2] = polygon;
            _triangles[3 * polygon - 1] = 1;

            mesh.Clear();
            mesh.vertices = _vertices;
            mesh.triangles = _triangles;
            mesh.RecalculateNormals();
        }

        private void OnValidate()
        {
            if (renderer == null)
                renderer = GetComponent<MeshRenderer>();

            if (mesh == null)
            {
                mesh = new Mesh();

                var filter = GetComponent<MeshFilter>();
                filter.mesh = mesh;
            }
            else
                UpdateMesh();
        }
    }
}
