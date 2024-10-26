using System;
using Cysharp.Threading.Tasks;
using ThreeMatch.Core;
using ThreeMatch.Firebase.Data;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Popup;
using ThreeMatch.Server;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.OutGame.Presenter
{    
    public class DailyRewardPresenter : BasePresenter
    {
        private DailyPopup _popup;
        private DailyRewardModel _model;
        private UserModel _userModel;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        
        public void Initialize(DailyPopup popup, DailyRewardModel rewardModel, UserModel userModel)
        {
            _popup = popup;
            _model = rewardModel;
            _userModel = userModel;

            _disposable.Clear();
            _model.dailyRewardItemDataList.Subscribe(list =>
            {
                // 현재 시간 -마지막으로 받은시간 : 하루 지났을 떄 받을 수 있도록하기 + 마지막 인덱스만 받을 수 있도록 하기 
                bool isPossibleGetReward = (DateTime.UtcNow - _model.lastReceivedRewardTime).TotalDays >= 1;
                // bool isPossibleGetReward = true;
                int possibleGetRewardItemFirstIndex = list.FindIndex(v => !v.IsGetReward);

                _popup.Initialize(list, isPossibleGetReward, possibleGetRewardItemFirstIndex, OnGetReward, OnGetReward);

                Debug.Log(possibleGetRewardItemFirstIndex);
                if ((isPossibleGetReward && possibleGetRewardItemFirstIndex != -1) ||
                    possibleGetRewardItemFirstIndex == 0)
                {
                    _popup.Open();
                }

            }).AddTo(_disposable);

            // _popup.Open();
        }

        private async void OnGetReward()
        {
            var item =_model.dailyRewardItemDataList.Value.Find(v => !v.IsGetReward);
            if (item == null)
            {
                return;
            }

            await RequestGetDailyReward(item.ItemId);
        }

        private async void OnGetReward(int itemId)
        {
            await RequestGetDailyReward(itemId);
        }
        
        private async UniTask RequestGetDailyReward(int itemId)
        {
            var response = await ServerHandlerFactory.Get<ServerUserRequestHandler>().GetDailyRewardRequest(itemId);
            if (response.responseCode != ServerErrorCode.Success)
            {
                switch (response.responseCode)
                {
                    case ServerErrorCode.FailedGetData:
                    case ServerErrorCode.FailedGetDailyReward:
                    //Alret
                    case ServerErrorCode.FailedFirebaseError:
                        SceneManager.LoadScene(SceneType.Title.ToString());
                        return;
                }
            }

            DailyRewardHistoryData dailyRewardHistoryData = response.dailyRewardHistoryData;
            _userModel.money.Value = response.userMoney;
            _model.CreateDailyRewardItemList(dailyRewardHistoryData);

            await UniTask.WaitForSeconds(1f);
            
            _popup.Close();
        }
    }
}