using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.Interface;
using ThreeMatch.OutGame.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.Popup
{
    public class ShopPopup : MonoBehaviour, IPopup
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
        [SerializeField] private List<ItemPackElement> _itemPackElementList;
        [SerializeField] private ScrollRect _scrollRect;
        
        public void Initialize(Action<int> onBuyProductAction, List<GoldPackElementData> goldPackElementDataList)
        {
            // for (var i = 0; i < _goldPackElementList.Count; i++)
            // {
            //     GoldPackElementData data = goldPackElementDataList[i];
            //     GoldPackElement element = _goldPackElementList[i];
            //     element.Initialize(data.goldAmount, data.price, onBuyProductAction);
            // }

            var gridLayoutGroup = _scrollRect.content.GetComponent<GridLayoutGroup>();
            int count = _goldPackElementList.Count + _itemPackElementList.Count;
            float y = count * gridLayoutGroup.spacing.y + count * gridLayoutGroup.cellSize.y + 100;
            _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, y);
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