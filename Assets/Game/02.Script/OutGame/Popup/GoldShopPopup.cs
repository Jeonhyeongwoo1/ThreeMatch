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
        [SerializeField] private Button _closeButton;
        [SerializeField] private List<GoldPackElement> _goldPackElementList;

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