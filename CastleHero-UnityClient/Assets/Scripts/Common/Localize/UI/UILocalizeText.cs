using RGLabs.Data;
using TMPro;
using UnityEngine;

namespace RGLabs.Common.Localize.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UILocalizeText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        public int id;

        private void Awake() => Storage.localize.OnLoaded += Refresh;

        private void OnDestroy() => Storage.localize.OnLoaded -= Refresh;

        private void OnEnable() => Refresh();

        private void Refresh() => _label.text = id.Localize();

        private void OnValidate()
        {
            if (_label == null)
                _label = GetComponent<TMP_Text>();
        }
    }
}