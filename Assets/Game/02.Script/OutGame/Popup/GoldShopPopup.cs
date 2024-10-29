using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.Interface;
using ThreeMatch.OutGame.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Popup
{
    public class GoldShopPopup : MonoBehaviour, IPopup
    {
        [Serializable]
        public struct GoldPackElementData
        {
            public int itemId;
            public int goldAmount;
            public float price;
        }
        
        [SerializeField] private Button _closeButton;
        [SerializeField] private List<GoldPackElement> _goldPackElementList;

        public void Initialize(Action<int> onBuyProductAction, List<GoldPackElementData> goldPackElementDataList)
        {
            // for (var i = 0; i < _goldPackElementList.Count; i++)
            // {
            //     GoldPackElementData data = goldPackElementDataList[i];
            //     GoldPackElement element = _goldPackElementList[i];
            //     element.Initialize(data.goldAmount, data.price, onBuyProductAction);
            // }
        }
        
        private void Start()
        {
            _closeButton.onClick.AddListener(OnClickClose);
        }

        private void OnClickClose()
        {
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }
    }
}