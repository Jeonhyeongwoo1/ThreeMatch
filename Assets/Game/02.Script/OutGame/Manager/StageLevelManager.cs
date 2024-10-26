using System;
using Cysharp.Threading.Tasks;
using ThreeMatch.Core;
using ThreeMatch.InGame.Manager;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Entity;
using ThreeMatch.OutGame.Popup;
using ThreeMatch.OutGame.Presenter;
using ThreeMatch.OutGame.View;
using ThreeMatch.Server;
using ThreeMatch.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.OutGame.Manager
{
    public class StageLevelManager : MonoBehaviour
    {
        [SerializeField] private StageLevelListView _stageLevelListView;
        [SerializeField] private WaypointsMover _waypointsMover;
        [SerializeField] private GameObject _idleParticleObj;
        [SerializeField] private ScreenFader _screenFader;
        
        private StageLevelListPresenter _stageLevelListPresenter;

        public static Action<int, Vector3> onArrivedWayPointAction;
        public static Action onStartMoveWayPointAction;
        public static Action onSelectedStageAction;
        
        private void Start()
        {
            LoadStageLevel();
            Initialize();
        }

        private void OnFinishedDestinationWayPoint(Vector3 position)
        {
            int level = 0;
            // _idleParticleObj.SetActive(true);
            onArrivedWayPointAction?.Invoke(level, position);
            _stageLevelListPresenter.UnLockStageLevel(level);
        }

        private void Initialize()
        {
            var userModel = ModelFactory.CreateOrGet<UserModel>();
            UIManager uiManager = UIManager.Instance;
            var userHeartPresenter = PresenterFactory.CreateOrGet<UserInfoPresenter>();
            userHeartPresenter.Initialize(uiManager.GetView<UserInfoView>(), userModel);
            userHeartPresenter.UpdateUserHeart();

            PopupManager popupManager = PopupManager.Instance;
            var dailyRewardPresenter = PresenterFactory.CreateOrGet<DailyRewardPresenter>();
            dailyRewardPresenter.Initialize(popupManager.GetPopup<DailyPopup>(),
                ModelFactory.CreateOrGet<DailyRewardModel>(), userModel);
        }

        private async void OnSelectStage(int level)
        {
            var userModel = ModelFactory.CreateOrGet<UserModel>();
            if (userModel.heart.Value == 0)
            {
                return;
            }

            var response = await ServerHandlerFactory.Get<ServerStageRequestHandler>().RemoveHeartRequest();
            if (response.responseCode != ServerErrorCode.Success)
            {
                switch (response.responseCode)
                {
                    case ServerErrorCode.FailedGetUserData:
                        Debug.LogError("Failed get user data");
                        return;
                    case ServerErrorCode.NotEnoughHeart:
                        Debug.Log("heart 수가 부족합니다.");
                        //Alert popup
                        return;
                }
            }

            userModel.heart.Value = response.userData.Heart;
            onSelectedStageAction?.Invoke();
            await _screenFader.FadeOut();

            var stageLevelListModel = ModelFactory.CreateOrGet<StageLevelListModel>();
            stageLevelListModel.selectedStageLevel = level;
            var operation = SceneManager.LoadSceneAsync(SceneType.InGame.ToString());
            // while (!operation.isDone)
            // {
            //     await UniTask.Yield();
            // }

            Debug.Log("level : " + level);            
        }
        
        private async UniTask LoadStageLevel()
        {
            var stageLevelListModel = ModelFactory.CreateOrGet<StageLevelListModel>();
            if (stageLevelListModel.stageLevelModelList == null)
            {
                var response = await ServerHandlerFactory.Get<ServerStageRequestHandler>().LoadStageDataRequest();
                if (response.responseCode != ServerErrorCode.Success)
                {
                    switch (response.responseCode)
                    {
                        case ServerErrorCode.FailedGetData:
                            Debug.LogError("Failed loadStageLevel :" + response.errorMessage);
                            SceneManager.LoadScene(SceneType.Title.ToString());
                            return;
                    }
                }
                
                stageLevelListModel.AddStageLevelModelList(response.stageLevelDataList);    
            }
            
            _stageLevelListPresenter = PresenterFactory.CreateOrGet<StageLevelListPresenter>();
            _stageLevelListPresenter.Initialize(_stageLevelListView, stageLevelListModel, OnSelectStage);

            int lastStageLevelIndex = 0;
            try
            {
                lastStageLevelIndex = stageLevelListModel.stageLevelModelList.Value.FindLastIndex(v => !v.isLock);
            }
            catch (Exception e)
            {
                lastStageLevelIndex = 0;
                Debug.LogError("failed last index " + e);
            }
            
            if (stageLevelListModel.openNewStage)
            {
                stageLevelListModel.openNewStage = false;
                int prevIndex = lastStageLevelIndex - 1;
                try
                {
                    await UniTask.WaitForSeconds(1f, cancelImmediately: true);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("failed task wait : " + e);
                }
                _waypointsMover.Move(prevIndex, lastStageLevelIndex, OnFinishedDestinationWayPoint);
            }
            else
            {
                _waypointsMover.SetPosition(lastStageLevelIndex);
            }
        }
    }
}