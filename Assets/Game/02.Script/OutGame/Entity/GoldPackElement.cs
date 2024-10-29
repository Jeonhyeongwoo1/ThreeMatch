using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Entity
{
    public class GoldPackElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _goldAmountText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _button;

        private int _productId;
        private Action<int> _onBuyProductAction;
        
        private void Start()
        {
            _button.onClick.AddListener(()=> _onBuyProductAction.Invoke(_productId));
        }

        public void Initialize(int goldAmount, float price, Action<int> onBuyProductAction)
        {
            _onBuyProductAction = onBuyProductAction;
            UpdateUI(goldAmount.ToString(), price.ToString(CultureInfo.InvariantCulture));
        }

        private void UpdateUI(string goldAmount, string price)
        {
            _goldAmountText.text = goldAmount;
            _priceText.text = price;
        }
    }
}