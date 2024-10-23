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
    public class UserHeartPresenter : BasePresenter
    {
        private UserModel _model;
        private UserHeartView _view;

        public void Initialize(UserHeartView view, UserModel model)
        {
            _model = model;
            _view = view;
            _view.Initialize(OpenHeartShopPopup, Const.MaxUserHeartCount <= _model.heart.Value);
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