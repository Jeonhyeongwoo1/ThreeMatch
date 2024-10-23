using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using ThreeMatch.OutGame.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.View
{
    public class UserHeartView : MonoBehaviour, IView
    {
        [SerializeField] private TextMeshProUGUI _heartCountText;
        [SerializeField] private Button _chargeButton;
        [SerializeField] private Timer _heartTimer;

        private Action _onOpenHeartShopPopupAction;
        
        public void Initialize(Action onOpenHeartShopPopupAction, bool isMaxHeart)
        {
            _onOpenHeartShopPopupAction = onOpenHeartShopPopupAction;
            _chargeButton.gameObject.SetActive(!isMaxHeart);
        }
        
        private void Start()
        {
            _chargeButton.onClick.AddListener(OnClickChargeButton);
        }

        private void OnClickChargeButton()
        {
            _onOpenHeartShopPopupAction?.Invoke();
        }

        public void UpdateHeartCount(string count, bool isMax)
        {
            _heartCountText.text = count;
            _chargeButton.gameObject.SetActive(!isMax);
        }

        public void StartHeartChargeTimer(DateTime chargeTime, Action onChargedHeartAction)
        {
            _heartTimer.StartTimer(chargeTime, onChargedHeartAction);
        }

        public void UpdateHeartTimerText(string value)
        {
            _heartTimer.UpdateTimerText(value);
        }

        public void StopHeartChargeTimer()
        {
            _heartTimer.StopTimer();
        }
    } 
}