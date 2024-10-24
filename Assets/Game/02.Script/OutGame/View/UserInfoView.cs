using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using ThreeMatch.OutGame.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ThreeMatch.OutGame.View
{
    public class UserInfoView : MonoBehaviour, IView
    {
        [SerializeField] private TextMeshProUGUI _heartCountText;
        [SerializeField] private Button _heartChargeButton;
        [SerializeField] private Timer _heartTimer;
        [SerializeField] private Button _goldChargeButton;
        [SerializeField] private TextMeshProUGUI _goldCountText;

        private Action _onOpenHeartShopPopupAction;
        private Action _onOpenGoldShopPopupAction;
        
        public void Initialize(Action onOpenHeartShopPopupAction, Action onOpenGoldShopPopupAction, bool isMaxHeart)
        {
            _onOpenHeartShopPopupAction = onOpenHeartShopPopupAction;
            _onOpenGoldShopPopupAction = onOpenGoldShopPopupAction;
            _heartChargeButton.gameObject.SetActive(!isMaxHeart);
        }
        
        private void Start()
        {
            _heartChargeButton.onClick.AddListener(OnClickHeartChargeButton);
            _goldChargeButton.onClick.AddListener(OnClickGoldChargeButton);
        }

        private void OnDisable()
        {
            StopHeartChargeTimer();
        }

        private void OnClickGoldChargeButton()
        {
            _onOpenGoldShopPopupAction?.Invoke();
        }

        private void OnClickHeartChargeButton()
        {
            _onOpenHeartShopPopupAction?.Invoke();
        }

        public void UpdateGold(string value)
        {
            _goldCountText.text = value;
        }

        public void UpdateHeartCount(string count, bool isMax)
        {
            _heartCountText.text = count;
            _heartChargeButton.gameObject.SetActive(!isMax);
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