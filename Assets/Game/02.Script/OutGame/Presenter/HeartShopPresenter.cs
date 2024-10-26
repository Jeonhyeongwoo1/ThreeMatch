using System;
using ThreeMatch.Core;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Popup;
using ThreeMatch.Server;
using ThreeMatch.Title.Controller;
using UniRx;
using UnityEngine;

namespace ThreeMatch.OutGame.Presenter
{
    public class HeartShopPresenter : BasePresenter
    {
        private UserModel _model;
        private HeartShopPopup _popup;

        private CompositeDisposable _disposable = new();
        
        public void Initialize(UserModel userModel, HeartShopPopup heartShopPopup)
        {
            _model = userModel;
            _popup = heartShopPopup;
            _popup.Initialize(OnBuyLift, OnShowAd);

            _disposable.Clear();
            _model.heart.Subscribe(OnHeartSubScribe).AddTo(_disposable);
        }

        private void OnHeartSubScribe(int heart)
        {
            CheckIfNeedToChargeHeart(heart);
        }

        public void OpenHeartShopPopup()
        {
            int heartCount = _model.heart.Value;
            int numberOfHeartNeeded = Const.MaxUserHeartCount - heartCount;
            _popup.Open(_model.heartRechargeTime.Value, Const.HeartPurchaseCost.ToString(), OnChargedHeart);
        }

        private void OnChargedHeart()
        {
            int heartCount = _model.heart.Value;
            CheckIfNeedToChargeHeart(heartCount);
        }
        
        private void CheckIfNeedToChargeHeart(int heartCount)
        {
            if (heartCount < Const.MaxUserHeartCount)
            {
                DateTime chargeTime = _model.heartRechargeTime.Value;
                // DateTime chargeTime = DateTime.UtcNow.AddMinutes(Const.HeartChargeMinute);
                _model.heartRechargeTime.Value = chargeTime;
                _popup.StarHeartChargeTimer(_model.heartRechargeTime.Value, OnChargedHeart);
            }
            else
            {
                _popup.Close();
            }
        }
        
        private async void OnBuyLift()
        {
            var response = await ServerHandlerFactory.Get<ServerUserRequestHandler>().BuyHeartRequest();
            switch (response.responseCode)
            {
                case ServerErrorCode.Success:
                    break;
                case ServerErrorCode.MaxHeartCount:
                    return;
                case ServerErrorCode.NotEnoughMoney:
                    return;
            }

            int heartCount = response.userData.Heart;
            _model.money.Value = response.userData.Money;
            _model.heart.Value = heartCount;
            if (heartCount == Const.MaxUserHeartCount)
            {
                _model.heartRechargeTime.Value = DateTime.MinValue;
                _popup.Close();
            }
            
            CheckIfNeedToChargeHeart(heartCount);
        }

        private void OnShowAd()
        {
            AdManager.Instance.ShowRewardAd(OnSuccessShowRewardAd, null);
        }

        private async void OnSuccessShowRewardAd()
        {
            var response = await ServerHandlerFactory.Get<ServerUserRequestHandler>().ChargedHeartRequest();
            if (response.responseCode != ServerErrorCode.Success)
            {
                switch (response.responseCode)
                {
                    case ServerErrorCode.FailedGetUserData:
                        //Alert
                        return;
                }
            }

            int heart = response.userData.Heart;
            _model.heart.Value = heart;
            CheckIfNeedToChargeHeart(heart);
        }
    }
}
