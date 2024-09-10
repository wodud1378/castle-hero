using System;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Unit.Behaviours;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UICastleState : MonoBehaviour
    {
        [SerializeField] private TMP_Text _lv;
        [SerializeField] private TMP_Text _hp;
        [SerializeField] private Slider _hpBar;
        [SerializeField] private GameObject _root;

        private UnitBehaviour _castle;

        private void Awake()
        {
            _root.SetActive(false);
            
            Storage.inGameRepository.castle
                .Subscribe(OnCastleChanged)
                .AddTo(this);
        }

        private void OnCastleChanged(UnitBehaviour castle)
        {
            _castle = castle;
            if (_castle == null)
            {
                _root.SetActive(false);
                return;
            }
            
            _root.SetActive(true);
            _castle.information
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    _lv.text = $"{x.lv}";
                })
                .AddTo(_castle);
                
            var hp = _castle.status.hp;
            _castle
                .UpdateAsObservable()
                .Subscribe(_ =>
                {
                    float left = hp.Left;
                    float max = hp.Max;
                    float ratio = left / max;

                    _hpBar.value = ratio;
                    _hp.text = $"{(int)left:N0}/{(int)max:N0}";
                })
                .AddTo(_castle);
        }
    }
}