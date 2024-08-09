using System.Collections.Generic;
using BackEnd;
using RGLabs.Data;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Network.Service.Test
{
    public class UITest : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _closeButton;
        
        [Header("아이템 추가")] 
        [SerializeField] private TMP_InputField _itemId;
        [SerializeField] private TMP_InputField _itemQty;
        [SerializeField] private Button _addItemButton;
        [SerializeField] private TMP_Text _addItemLog;
        
        [Header("재화 추가")] 
        [SerializeField] private TMP_InputField _paidDia;
        [SerializeField] private TMP_InputField _freeDia;
        [SerializeField] private TMP_InputField _gold;
        [SerializeField] private Button _addCurrencyButton;
        [SerializeField] private TMP_Text _addCurrencyLog;
        
        private readonly TestService _service = new();

        private bool IsOpened => _root.activeSelf;

        private void Awake()
        {
            this.SubscribeButton(_closeButton, Close);
            this.SubscribeButton(_addItemButton, OnClickAddItems);
            this.SubscribeButton(_addCurrencyButton, OnClickAddCurrency);
            
            Close();
        }
        
        private async void OnClickAddItems()
        {
            var ids = _itemId.text.Trim().Split(',');
            var quantities = _itemQty.text.Trim().Split(',');
            var idList = new List<int>();
            var quantityList = new List<int>();

            int index = 0;
            while (index.IsValidIndex(ids, quantities))
            {
                if (int.TryParse(ids[index], out int id) &&
                    int.TryParse(quantities[index], out int quantity))
                {
                    idList.Add(id);
                    quantityList.Add(quantity);
                }
                
                ++index;
            }

            if (idList.Count == 0 || quantityList.Count == 0)
            {
                _addItemLog.text = "입력 오류".WithNegativeColor();
                return;
            }
            
            _addItemLog.text = "요청 진행 중".WithColor(Color.gray);

            var result = await _service.AddItems(idList.ToArray(), quantityList.ToArray());
            
            Storage.userRepository.inventory.Update(result);
            
            _addItemLog.text = "요청 성공".WithPositiveColor();
        }

        private async void OnClickAddCurrency()
        {
            int temp;
            int paidDia = int.TryParse(_paidDia.text, out  temp) ? temp : 0;
            int freeDia = int.TryParse(_freeDia.text, out  temp) ? temp : 0;
            int gold = int.TryParse(_gold.text, out  temp) ? temp : 0;
            
            if (paidDia == 0 && freeDia == 0 && gold == 0)
            {
                _addCurrencyLog.text = "입력 오류".WithNegativeColor();
            }
            else
            {
                _addCurrencyLog.text = "요청 진행 중".WithColor(Color.gray);
                var currency = new CurrencyDto { paidDia = paidDia, freeDia = freeDia, gold = gold, };

                var result = await _service.AddCurrency(currency);
                
                Storage.userRepository.currency.Update(result);

                _addCurrencyLog.text = "요청 성공".WithPositiveColor();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && IsOpened)
            {
                Close();
            }

            if (Input.GetKeyDown(KeyCode.F1) && !IsOpened)
            {
                Open();
            }
        }

        public void Open() => _root.SetActive(true);

        private void Close() => _root.SetActive(false);
    }
}