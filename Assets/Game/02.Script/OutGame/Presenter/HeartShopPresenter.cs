using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Popup;
using UnityEngine;

namespace ThreeMatch.OutGame.Presenter
{
    public class HeartShopPresenter : BasePresenter
    {
        private UserModel _model;
        private HeartShopPopup _popup;
        
        public void Initialize(UserModel userModel, HeartShopPopup heartShopPopup)
        {
            _model = userModel;
            _popup = heartShopPopup;
            _popup.Initialize(OnBuyLift, OnShowAd);
        }

        public void OpenHeartShopPopup()
        {
            int heartCount = _model.heart.Value;
            int numberOfHeartNeeded = Const.MaxUserHeartCount - heartCount;
            _popup.Open(_model.heartRechargeTime.Value, Const.HeartPurchaseCost.ToString(), OnChargedHeart);
        }

        private void OnChargedHeart()
        {
            _model.heart.Value++;
            int heartCount = _model.heart.Value;
            CheckIfNeedToChargeHeart(heartCount);
        }
        
        private void CheckIfNeedToChargeHeart(int heartCount)
        {
            if (heartCount < Const.MaxUserHeartCount)
            {
                DateTime chargeTime = DateTime.UtcNow.AddSeconds(Const.HeartChargeMinute + 10);
                // DateTime chargeTime = DateTime.UtcNow.AddMinutes(Const.HeartChargeMinute);
                _model.heartRechargeTime.Value = chargeTime;
                _popup.StarHeartChargeTimer(_model.heartRechargeTime.Value, OnChargedHeart);
            }
            else
            {
                _popup.Close();
            }
        }
        
        private void OnBuyLift()
        {
            int heartCount = _model.heart.Value;
            // if (Const.MaxUserHeartCount <= heartCount || _model.money.Value < Const.HeartPurchaseCost)
            // {
            //     return;
            // }

            _model.money.Value -= Const.HeartPurchaseCost;
            _model.heart.Value++;
            heartCount++;
            if (heartCount == Const.MaxUserHeartCount)
            {
                _model.heartRechargeTime.Value = DateTime.MinValue;
                _popup.Close();
            }
            
            CheckIfNeedToChargeHeart(heartCount);
            var userHeartPresenter = PresenterFactory.CreateOrGet<UserHeartPresenter>();
            userHeartPresenter.UpdateUserHeart();
        }

        private void OnShowAd()
        {
            
        }
    }
}