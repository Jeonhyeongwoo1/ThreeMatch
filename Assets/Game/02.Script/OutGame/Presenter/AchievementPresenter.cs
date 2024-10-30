using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.Common.Data;
using ThreeMatch.Core;
using ThreeMatch.Firebase.Data;
using ThreeMatch.InGame.Manager;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Entity;
using ThreeMatch.OutGame.Popup;
using ThreeMatch.Server;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.OutGame.Presenter
{
    public class AchievementPresenter : BasePresenter
    {
        private AchievementPopup _popup;
        private AchievementModel _achieveModel;
        private UserModel _userModel;
        private readonly CompositeDisposable _disposable = new();

        public void Initialize(AchievementModel achieveModel, UserModel userModel, AchievementPopup popup)
        {
            _achieveModel = achieveModel;
            _userModel = userModel;
            _popup = popup;

            _popup.Initialize(_achieveModel.achievementDataList.Value, OnRewardGet);
            _disposable?.Clear();
            _achieveModel.achievementDataList.Subscribe(UpdateUI).AddTo(_disposable);
        }

        private async void OnRewardGet(int achievementId, AchievementElement element)
        {
            var response = await ServerHandlerFactory.Create<ServerUserRequestHandler>()
                .GetAchievementRewardRequest(achievementId);

            if (response.responseCode != ServerErrorCode.Success)
            {
                switch (response.responseCode)
                {
                    case ServerErrorCode.FailedFirebaseError:
                    case ServerErrorCode.FailedGetAchievement:
                        Debug.LogError($"server error : " + response.errorMessage);
                        SceneManager.LoadScene(SceneType.Title.ToString());
                        return;
                    case ServerErrorCode.NotMatchedAchievementId:
                    case ServerErrorCode.AlreadyAchievementId:
                        //Alert
                        SceneManager.LoadScene(SceneType.Title.ToString());
                        return;
                }
            }

            _achieveModel.SetAchievementDataList(response.achievementHistoryData.AchievementDataList);
            _popup.ActiveGoldObj(true);
            long prev = _userModel.money.Value;
            _popup.UpdateUserGold(prev, 0, false);
            _userModel.money.Value = response.money;

            UniTaskCompletionSource source = new UniTaskCompletionSource();
            TokenManager.Instance.GenerateGoldToken(element.GoldSpawnPosition, _popup.GoldTextPosition, 10, source);

            await source.Task;

            _popup.UpdateUserGold(response.money, prev);
        }

        private void UpdateUI(List<AchievementData> achievementDataList)
        {
            if (_popup == null)
            {
                return;
            }
            
            _popup.UpdateAchievementElementList(achievementDataList);
        }
    }
}