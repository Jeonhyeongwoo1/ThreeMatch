using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Entity
{
    public class ItemPurchaseElement : MonoBehaviour
    {
        [SerializeField] private InGameItemType _itemType;
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _itemCountText;
        [SerializeField] private GameResourcesConfigData _configData;

        private void Start()
        {
            _itemImage.sprite = _configData.GetItemSprite(_itemType);
        }

        public void UpdateUI(string itemCount)
        {
            _itemCountText.text = itemCount;
        }
    }
}