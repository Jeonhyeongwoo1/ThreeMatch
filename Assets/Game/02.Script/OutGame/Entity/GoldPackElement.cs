using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ThreeMatch.OutGame.Entity
{
    public class GoldPackElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _goldAmountText;
        [SerializeField] private TextMeshProUGUI _priceText;

        public void UpdateUI(string goldAmount, string price)
        {
            _goldAmountText.text = goldAmount;
            _priceText.text = price;
        }
    }
}