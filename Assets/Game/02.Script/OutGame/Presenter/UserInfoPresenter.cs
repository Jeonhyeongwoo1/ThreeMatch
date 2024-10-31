using System;
using ThreeMatch.Core;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Popup;
using ThreeMatch.OutGame.View;
using ThreeMatch.Server;
using UniRx;
using UnityEngine;

namespace ThreeMatch.OutGame.Presenter
{
    public class UserInfoPresenter : BasePresenter, IDisposable
    {
        private UserModel _model;
        private UserInfoView _view;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        
        public void Initialize(UserInfoView view, UserModel model)
        {
            _model = model;
            _view = view;
            _view.Initialize(OpenHeartShopPopup, OpenGoldShopPopup, Const.MaxUserHeartCount <= _model.heart.Value);

            _compositeDisposable?.Clear();
            _model.heart.Subscribe(v => UpdateUserHeart()).AddTo(_compositeDisposable);
            _model.money.Subscribe(v => _view.UpdateGold(v.ToString())).AddTo(_compositeDisposable);
        }

        private void OpenGoldShopPopup()
        {
            var goldShopPopup= PopupManager.Instance.GetPopup<ShopPopup>();
            if (goldShopPopup != null)
            {
                goldShopPopup.Open();
            }
        }

        private void OpenHeartShopPopup()
        {
            if (Const.MaxUserHeartCount <= _model.heart.Value)
            {
                return;
            }
            
            EventManager.RaiseEvent(nameof(HeartShopPresenter.OpenHeartShopPopup));
        }

        public void UpdateUserHeart()
        {
            int heartCount = _model.heart.Value;
            
            _view.UpdateHeartCount(heartCount.ToString(), heartCount == Const.MaxUserHeartCount);
            CheckIfNeedToChargeHeart(heartCount);
        }

        private async void OnChargedHeart()
        {
            int heartCount = _model.heart.Value;
            var res = await ServerHandlerFactory.Get<ServerUserRequestHandler>()
                .ChargedHeartRequest();

            if (res.responseCode != ServerErrorCode.Success)
            {
                return;
            }

            _model.heart.Value = res.userData.Heart;
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

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}