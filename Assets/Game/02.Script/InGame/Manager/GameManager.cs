using System;
using Cysharp.Threading.Tasks;
using ThreeMatch.Common.Data;
using ThreeMatch.Core;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.InGame.UI;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Data;
using ThreeMatch.Server;
using ThreeMatch.Shared;
using ThreeMatch.Shared.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static Action<InGameItemType> onInGameItemUsagePendingAction;
        public static Action<InGameItemType> onUsedInGameItemAction;
        public static Action onAllSuccessMissionAction;
        public static Action onGameReadyAction;
        public static Action onGameStartAction;
        public static Action<GameResultData> onGameClearAction;
        public static Action<GameResultData> onGameOverAction;
        public static Action<int> onChangeRemainingMoveCountAction;
        public static Action<int, int> onCellComboAction;
    
        private InGameItemPresenter _inGameItemPresenter;
        private GameFailPresenter _gameFailPresenter;
        private GameWinPresenter _gameWinPresenter;
        private GameState _gameState;

        private void Awake()
        {
            Initialized();
            InGameMenuButtonCallbackInitialize();
        }

        private void OnEnable()
        {
            onGameReadyAction += OnGameReady;
            onGameStartAction += OnGameStart; 
            onGameOverAction += OnGameOver;
            onGameClearAction += OnGameClear;
        }

        private void OnDestroy()
        {
            onGameReadyAction -= OnGameReady;
            onGameStartAction -= OnGameStart;
            onGameOverAction -= OnGameOver;
            onGameClearAction -= OnGameClear;
        }

        private void OnGameStart()
        {
            UpdateState(GameState.Start);
        }

        private void OnGameReady()
        {
            UpdateState(GameState.Ready);
        }

        private void Initialized()
        {
            UIManager uiManager = UIManager.Instance;

            _inGameItemPresenter = PresenterFactory.CreateOrGet<InGameItemPresenter>();
            _inGameItemPresenter.Initialize(uiManager.GetView<InGameItemView>());
            
            _gameFailPresenter = PresenterFactory.CreateOrGet<GameFailPresenter>();
            var gameFailedView = PopupManager.Instance.GetPopup<GameFailedPopup>();
            _gameFailPresenter.Initialize(gameFailedView, ModelFactory.CreateOrGet<MissionModel>());
            
            _gameWinPresenter = PresenterFactory.CreateOrGet<GameWinPresenter>();
            var gameWinPopup = PopupManager.Instance.GetPopup<GameWinPopup>();
            var missionModel = ModelFactory.CreateOrGet<MissionModel>();
            _gameWinPresenter.Initialize(gameWinPopup, missionModel);
        }

        private async void OnGameOver(GameResultData gameResultData)
        {
            if (GameState.Start != _gameState)
            {
                return;
            }
            
            var stageLevelModel = ModelFactory.CreateOrGet<StageLevelListModel>();
            int stageLevel = stageLevelModel.selectedStageLevel;
            
            UpdateState(GameState.End);
            StageResponse response = await StageClearOrFailRequest(gameResultData, false, stageLevel);
            stageLevelModel.AddStageLevelModelList(response.stageLevelDataList);

            var achievementModel = ModelFactory.CreateOrGet<AchievementModel>();
            achievementModel.SetAchievementDataList(response.achievementHistoryData.AchievementDataList);
            
            await _gameFailPresenter.GameFailProcess(gameResultData.starCount, stageLevel);
        }

        private async UniTask<StageResponse> StageClearOrFailRequest(GameResultData gameResultData, bool isClear, int stageLevel)
        {
            var ingameItemModel = ModelFactory.CreateOrGet<InGameItemModel>();
            var response = await ServerHandlerFactory.Get<ServerStageRequestHandler>()
                .StageClearOrFailRequest(stageLevel, gameResultData, isClear, ingameItemModel.ConvertToInGameItemDataList());

            if (response.responseCode != ServerErrorCode.Success)
            {
                switch (response.responseCode)
                {
                    case ServerErrorCode.FailedGetData:
                    case ServerErrorCode.FailedGetStageData:
                        //Alert
                        SceneManager.LoadScene(SceneType.Title.ToString());
                        return null;
                }
            }
            
            return response;
        }

        private async void OnGameClear(GameResultData gameResultData)
        {
            if (GameState.Start != _gameState)
            {
                return;
            }
            
            UpdateState(GameState.End);

            var stageLevelModel = ModelFactory.CreateOrGet<StageLevelListModel>();
            int stageLevel = stageLevelModel.selectedStageLevel;
            StageResponse response = await StageClearOrFailRequest(gameResultData, true, stageLevel);

            int lastStageLevel = response.stageLevelDataList.FindLastIndex(v => !v.IsLock);
            stageLevelModel.openNewStage = stageLevel + 1 == lastStageLevel;
            stageLevelModel.AddStageLevelModelList(response.stageLevelDataList);
            var achievementModel = ModelFactory.CreateOrGet<AchievementModel>();
            achievementModel.SetAchievementDataList(response.achievementHistoryData.AchievementDataList);

            await _gameWinPresenter.GameWinProcess(stageLevel, gameResultData.starCount);
        }

        private void InGameMenuButtonCallbackInitialize()
        {
            var ingameMenuButtonModel = ModelFactory.CreateOrGet<InGameMenuButtonModel>();
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.NextStage, OnChangeNextStage);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.Share, OnShare);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.MoveToStageLevelScene, OnMoveToStageLevelScene);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.RestartGame, OnRestartGame);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.ShowAd, OnShowAds);
        }
        
        private async void OnRestartGame()
        {
            Debug.Log("OnRestartGame");
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
            
            _gameFailPresenter.HideFailPopupObj();
            var stageLevel = ModelFactory.CreateOrGet<StageLevelListModel>();
            StageManager.Instance.ReloadStage(stageLevel.selectedStageLevel);
        }

        private async void OnMoveToStageLevelScene()
        {
            Debug.Log("OnMoveToStageLevelScene");
            await UIManager.Instance.ScreenFader.FadeOut();
            SceneManager.LoadSceneAsync(SceneType.StageLevel.ToString());
        }

        private void OnShare()
        {
            Debug.Log("OnShare");
        }
        
        private void OnShowAds()
        {
            Debug.Log("OnShowAds");
        }
        
        private async void OnChangeNextStage()
        {
            Debug.Log("OnChangeNextStage");
            // var model = ModelFactory.CreateOrGet<StageLevelListModel>();
            // model.selectedStageLevel++;
            await UIManager.Instance.ScreenFader.FadeOut();
            SceneManager.LoadSceneAsync(SceneType.StageLevel.ToString());
        }

        private void UpdateState(GameState gameState) => _gameState = gameState;
    }
}














