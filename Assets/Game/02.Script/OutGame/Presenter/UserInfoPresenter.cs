using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Popup;
using ThreeMatch.OutGame.View;

namespace ThreeMatch.OutGame.Presenter
{
    public class UserInfoPresenter : BasePresenter
    {
        private UserModel _model;
        private UserInfoView _view;

        public void Initialize(UserInfoView view, UserModel model)
        {
            _model = model;
            _view = view;
            _view.Initialize(OpenHeartShopPopup, OpenGoldShopPopup, Const.MaxUserHeartCount <= _model.heart.Value);
            UpdateUserHeart();
            _view.UpdateGold(_model.money.Value.ToString());
        }

        private void OpenGoldShopPopup()
        {
            var goldShopPresenter = PresenterFactory.CreateOrGet<GoldShopPresenter>();
            var goldShopPopup = PopupManager.Instance.GetPopup<GoldShopPopup>();
            goldShopPresenter.Initialize(_model, goldShopPopup);
            goldShopPresenter.OpenGoldShopPopup();
        }

        private void OpenHeartShopPopup()
        {
            if (Const.MaxUserHeartCount <= _model.heart.Value)
            {
                return;
            }
            
            var heartShopPresenter = PresenterFactory.CreateOrGet<HeartShopPresenter>();
            var heartShopPopup = PopupManager.Instance.GetPopup<HeartShopPopup>();
            heartShopPresenter.Initialize(_model, heartShopPopup);
            heartShopPresenter.OpenHeartShopPopup();
        }

        public void UpdateUserHeart()
        {
            int heartCount = _model.heart.Value;
            
            _view.UpdateHeartCount(heartCount.ToString(), heartCount == Const.MaxUserHeartCount);
            CheckIfNeedToChargeHeart(heartCount);
        }

        private void OnChargedHeart()
        {
            _model.heart.Value++;
            int heartCount = _model.heart.Value;
            _view.UpdateHeartCount(heartCount.ToString(), heartCount == Const.MaxUserHeartCount);
            CheckIfNeedToChargeHeart(heartCount);
        }

        private void CheckIfNeedToChargeHeart(int heartCount)
        {
            if (heartCount < Const.MaxUserHeartCount)
            {
                DateTime chargeTime = DateTime.UtcNow.AddSeconds(Const.HeartChargeMinute + 10);
                // DateTime chargeTime = DateTime.UtcNow.AddMinutes(Const.HeartChargeMinute);
                _model.heartRechargeTime.Value = chargeTime;
                _view.StartHeartChargeTimer(_model.heartRechargeTime.Value, OnChargedHeart);
            }
            else
            {
                _view.StopHeartChargeTimer();
                _view.UpdateHeartTimerText("FULL");
            }
        }
    }
}