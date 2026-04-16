using CastleHero.Common.Localize;
using CastleHero.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
namespace CastleHero.View.Localize
{
    [RequireComponent(typeof(TMP_Text))]
    public class UILocalizeText : MonoBehaviour
    {
        [FormerlySerializedAs("_label")]
        [SerializeField] private TMP_Text label;

        public int id;

        private LocalizeText _localize;

        private void Awake()
        {
            _localize = ServiceLocator.Get<LocalizeText>();
            _localize.OnLoaded += Refresh;
        }

        private void OnDestroy()
        {
            if (_localize != null)
                _localize.OnLoaded -= Refresh;
        }

        private void OnEnable() => Refresh();

        private void Refresh() => label.text = id.Localize();

        private void OnValidate()
        {
            if (label == null)
                label = GetComponent<TMP_Text>();
        }
    }
}
