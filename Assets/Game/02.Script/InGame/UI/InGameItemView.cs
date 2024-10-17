using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public class InGameItemView : MonoBehaviour, IView
    {
        [Serializable]
        public struct InGameItemElement
        {
            public InGameItemType itemType;
            public Button button;
            public TextMeshProUGUI amountText;
        }

        [SerializeField] private List<InGameItemElement> _inGameItemElemntList;

        public Action<InGameItemType> onPendingUseInGameItem;
        
        private void Start()
        {
            _inGameItemElemntList.ForEach(v =>
                v.button.onClick.AddListener(() => onPendingUseInGameItem.Invoke(v.itemType)));
        }

        public void UpdateInGameItemAmount(InGameItemType inGameItemType, string amount)
        {
            foreach (InGameItemElement element in _inGameItemElemntList)
            {
                if (element.itemType == inGameItemType)
                {
                    element.amountText.text = amount;
                    break;
                }
            }
        }
    }
}